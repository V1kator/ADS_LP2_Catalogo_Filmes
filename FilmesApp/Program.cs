// FILE: /src/FilmesApp/Program.cs
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SQLitePCL;
using FilmesApp.Repositories;
using FilmesApp.Services.TMDb;
using FilmesApp.Services.Weather;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Inicializa provider nativo SQLite
// Certifique-se de ter adicionado o pacote SQLitePCLRaw.bundle_e_sqlite3
SQLitePCL.Batteries_V2.Init();

// Configuração
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Serviços
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

// Registra repositório e serviços no DI (importante para resolver IFilmeRepository etc.)
builder.Services.AddScoped<IFilmeRepository, FilmeRepository>();
builder.Services.AddScoped<ITmdbApiService, TmdbApiService>();
builder.Services.AddScoped<IWeatherApiService, WeatherApiService>();

// HttpClient named clients
builder.Services.AddHttpClient("tmdb", c =>
{
    c.BaseAddress = new Uri("https://api.themoviedb.org/3/");
});
builder.Services.AddHttpClient("open-meteo", c =>
{
    c.BaseAddress = new Uri("https://api.open-meteo.com/");
});

// === Inicialização/Criação automática do arquivo DB a partir do script SQL (se necessário) ===
try
{
    var env = builder.Environment;
    var contentRoot = env.ContentRootPath; // ex: D:\Desktop\Projetos\LPV2\FilmesApp\FilmesApp
    var dataDir = Path.Combine(contentRoot, "Data");
    Directory.CreateDirectory(dataDir);

    var dbFile = Path.Combine(dataDir, "filmes.db");

    // Localiza o script create_database.sql em alguns caminhos prováveis
    string? scriptFile = null;
    var candidates = new[]
    {
        Path.Combine(contentRoot, "..", "..", "db", "create_database.sql"),
        Path.Combine(contentRoot, "..", "db", "create_database.sql"),
        Path.Combine(contentRoot, "db", "create_database.sql"),
        Path.Combine(contentRoot, "..", "..", "..", "db", "create_database.sql"), // tentativa extra
        Path.Combine(contentRoot, "..", "..", "..", "..", "db", "create_database.sql")
    };

    foreach (var cand in candidates)
    {
        if (File.Exists(cand))
        {
            scriptFile = Path.GetFullPath(cand);
            break;
        }
    }

    if (scriptFile == null)
    {
        builder.Logging.AddConsole();
        var logger = LoggerFactory.Create(lb => lb.AddConsole()).CreateLogger("DbInit");
        logger.LogWarning("create_database.sql não encontrado automaticamente. Procure por /db/create_database.sql na raiz do repositório.");
    }
    else
    {
        bool needCreate = false;
        if (!File.Exists(dbFile))
        {
            needCreate = true;
        }
        else
        {
            // verifica header para confirmar arquivo SQLite válido
            try
            {
                byte[] header = new byte[16];
                using (var fs = File.OpenRead(dbFile))
                {
                    fs.Read(header, 0, header.Length);
                }
                var headerText = System.Text.Encoding.ASCII.GetString(header);
                if (!headerText.Contains("SQLite format 3"))
                {
                    var bak = dbFile + $".bad.{DateTime.UtcNow:yyyyMMddHHmmss}";
                    File.Move(dbFile, bak);
                    needCreate = true;
                }
            }
            catch
            {
                var bak = dbFile + $".bad.{DateTime.UtcNow:yyyyMMddHHmmss}";
                try { File.Move(dbFile, bak); } catch { /* ignore */ }
                needCreate = true;
            }
        }

        if (needCreate)
        {
            var sql = File.ReadAllText(scriptFile);
            var commands = sql.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            using var conn = new SqliteConnection($"Data Source={dbFile}");
            conn.Open();
            foreach (var cmdText in commands)
            {
                var txt = cmdText.Trim();
                if (string.IsNullOrWhiteSpace(txt)) continue;
                using var cmd = conn.CreateCommand();
                cmd.CommandText = txt + ";";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    var logger = LoggerFactory.Create(lb => lb.AddConsole()).CreateLogger("DbInit");
                    logger.LogError(ex, "Erro ao executar comando SQL durante criação do DB: {msg}", ex.Message);
                }
            }
            conn.Close();
        }
    }
}
catch (Exception ex)
{
    // não interrompe a inicialização — apenas loga
    Console.WriteLine("Erro na inicialização automática do DB: " + ex.Message);
}

// === Build e configuração do pipeline ===
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Filme}/{action=Index}/{id?}");

// Root redirect: redireciona / para /Filme/Index
app.MapGet("/", context =>
{
    context.Response.Redirect("/Filme/Index");
    return System.Threading.Tasks.Task.CompletedTask;
});

// ---------- Diagnóstico: verifica e cria a tabela 'Filmes' se necessário ----------
try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    var contentRoot = app.Environment.ContentRootPath;
    var dataDir = Path.Combine(contentRoot, "Data");
    var dbPath = Path.Combine(dataDir, "filmes.db");

    logger.LogInformation("Verificando banco SQLite em: {dbPath}", dbPath);

    if (!File.Exists(dbPath))
    {
        logger.LogWarning("Arquivo de banco não encontrado em {dbPath}. A aplicação continuará, mas a tabela será criada quando o DB existir.", dbPath);
    }
    else
    {
        using var conn = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();

        // lista tabelas existentes
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
            using var reader = cmd.ExecuteReader();
            var tables = new List<string>();
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }
            logger.LogInformation("Tabelas encontradas no DB: {tables}", string.Join(", ", tables));
        }

        // verifica se tabela Filmes existe
        bool filmesExists = false;
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Filmes';";
            var result = cmd.ExecuteScalar();
            if (result != null && Convert.ToInt32(result) > 0) filmesExists = true;
        }

        if (!filmesExists)
        {
            logger.LogWarning("Tabela 'Filmes' não encontrada — criando tabela agora (CREATE TABLE IF NOT EXISTS).");
            using var cmdCreate = conn.CreateCommand();
            cmdCreate.CommandText = @"
                CREATE TABLE IF NOT EXISTS Filmes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TmdbId INTEGER NOT NULL,
                    Titulo TEXT NOT NULL,
                    Sinopse TEXT,
                    DataLancamento TEXT,
                    IdiomaOriginal TEXT,
                    Avaliacao REAL,
                    PosterPath TEXT,
                    Latitude REAL NOT NULL,
                    Longitude REAL NOT NULL
                );";
            cmdCreate.ExecuteNonQuery();
            logger.LogInformation("Tabela 'Filmes' criada com sucesso (ou já existia).");
        }
        else
        {
            logger.LogInformation("Tabela 'Filmes' já existe. OK.");
        }

        conn.Close();
    }
}
catch (Exception ex)
{
    // Log e continua
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Erro ao verificar/criar tabela no DB SQLite: {msg}", ex.Message);
}

//
// Lista endpoints para diagnóstico (opcional, ajuda a ver se controllers foram mapeados)
try
{
    var endpointSource = app.Services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    foreach (var ep in endpointSource.Endpoints)
    {
        logger.LogInformation("Mapped endpoint: {endpoint}", ep.DisplayName ?? ep.ToString());
    }
}
catch { /* ignore */ }

app.Run();
