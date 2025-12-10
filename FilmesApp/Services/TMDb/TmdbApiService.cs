using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FilmesApp.Models.TmdbDtos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FilmesApp.Services.TMDb
{
    /// <summary>
    /// Implementação do ITmdbApiService usando IHttpClientFactory, IMemoryCache e ILogger.
    /// Faz chamadas para /search/movie, /movie/{id}, /movie/{id}/images e /configuration.
    /// Aplica caches conforme requisitos (buscas 5min, detalhes/images 10min, configuration longa).
    /// </summary>
    public class TmdbApiService : ITmdbApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TmdbApiService> _logger;
        private readonly string _apiKey;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public TmdbApiService(IHttpClientFactory httpClientFactory,
                              IMemoryCache cache,
                              IConfiguration configuration,
                              ILogger<TmdbApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
            // lê chave do TMDB preferencialmente por variável de ambiente ou appsettings
            _apiKey = configuration["TmdbApiKey"] ?? Environment.GetEnvironmentVariable("TMDB_API_KEY") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogWarning("TMDb API key não encontrada. Configure 'TmdbApiKey' em appsettings.Development.json or env TMDB_API_KEY.");
            }
        }

        private HttpClient CreateClient() => _httpClientFactory.CreateClient("tmdb");

        private string BuildUrlWithKey(string path, string query = "")
        {
            var sb = new StringBuilder();
            sb.Append(path);
            sb.Append(path.Contains("?") ? "&" : "?");
            sb.Append($"api_key={_apiKey}");
            if (!string.IsNullOrEmpty(query))
            {
                sb.Append("&");
                sb.Append(query);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Busca filmes no TMDb (cache TTL = 5 minutos). Registra logs detalhados.
        /// </summary>
        public async Task<SearchResultDto?> SearchMoviesAsync(string query, int page = 1)
        {
            string cacheKey = $"tmdb_search::{query?.ToLowerInvariant()}::page::{page}";
            if (_cache.TryGetValue(cacheKey, out SearchResultDto cached))
            {
                _logger.LogInformation("TMDb Search cache HIT. Query: {query} Page: {page} Time: {time}", query, page, DateTime.UtcNow);
                return cached;
            }

            var client = CreateClient();
            var endpoint = BuildUrlWithKey("search/movie", $"query={Uri.EscapeDataString(query)}&page={page}");
            _logger.LogInformation("TMDb Search request. Endpoint: {endpoint} Query: {query} Page: {page} Time: {time}", endpoint, query, page, DateTime.UtcNow);

            try
            {
                var resp = await client.GetAsync(endpoint);
                var body = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation("TMDb Search response. StatusCode: {status} Endpoint: {endpoint} Time: {time}", (int)resp.StatusCode, endpoint, DateTime.UtcNow);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("TMDb Search failed. Status: {status} Body: {body}", (int)resp.StatusCode, body);
                    return null;
                }

                var result = JsonSerializer.Deserialize<SearchResultDto>(body, _jsonOptions);
                // cache por 5 minutos
                _cache.Set(cacheKey, result!, TimeSpan.FromMinutes(5));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during TMDb.SearchMoviesAsync. Endpoint: {endpoint} Query: {query} Page: {page} Time: {time}", endpoint, query, page, DateTime.UtcNow);
                return null;
            }
        }

        /// <summary>
        /// Obtém detalhes de filme (cache TTL = 10 minutos).
        /// </summary>
        public async Task<MovieDetailsDto?> GetMovieDetailsAsync(int movieId)
        {
            string cacheKey = $"tmdb_details::{movieId}";
            if (_cache.TryGetValue(cacheKey, out MovieDetailsDto cached))
            {
                _logger.LogInformation("TMDb Details cache HIT. MovieId: {id} Time: {time}", movieId, DateTime.UtcNow);
                return cached;
            }

            var client = CreateClient();
            var endpoint = BuildUrlWithKey($"movie/{movieId}");
            _logger.LogInformation("TMDb Details request. Endpoint: {endpoint} MovieId: {id} Time: {time}", endpoint, movieId, DateTime.UtcNow);

            try
            {
                var resp = await client.GetAsync(endpoint);
                var body = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation("TMDb Details response. StatusCode: {status} MovieId: {id} Time: {time}", (int)resp.StatusCode, movieId, DateTime.UtcNow);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("TMDb Details failed. MovieId: {id} Status: {status} Body: {body}", movieId, (int)resp.StatusCode, body);
                    return null;
                }

                var result = JsonSerializer.Deserialize<MovieDetailsDto>(body, _jsonOptions);
                // cache por 10 minutos
                _cache.Set(cacheKey, result!, TimeSpan.FromMinutes(10));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during TMDb.GetMovieDetailsAsync. MovieId: {id} Time: {time}", movieId, DateTime.UtcNow);
                return null;
            }
        }

        /// <summary>
        /// Obtém imagens do filme (cache TTL = 10 minutos). Mapeia para MovieImagesDto.
        /// </summary>
        public async Task<MovieImagesDto?> GetMovieImagesAsync(int movieId)
        {
            string cacheKey = $"tmdb_images::{movieId}";
            if (_cache.TryGetValue(cacheKey, out MovieImagesDto cached))
            {
                _logger.LogInformation("TMDb Images cache HIT. MovieId: {id} Time: {time}", movieId, DateTime.UtcNow);
                return cached;
            }

            var client = CreateClient();
            var endpoint = BuildUrlWithKey($"movie/{movieId}/images");
            _logger.LogInformation("TMDb Images request. Endpoint: {endpoint} MovieId: {id} Time: {time}", endpoint, movieId, DateTime.UtcNow);

            try
            {
                var resp = await client.GetAsync(endpoint);
                var body = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation("TMDb Images response. StatusCode: {status} MovieId: {id} Time: {time}", (int)resp.StatusCode, movieId, DateTime.UtcNow);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("TMDb Images failed. MovieId: {id} Status: {status} Body: {body}", movieId, (int)resp.StatusCode, body);
                    return null;
                }

                var result = JsonSerializer.Deserialize<MovieImagesDto>(body, _jsonOptions);
                // cache por 10 minutos
                _cache.Set(cacheKey, result!, TimeSpan.FromMinutes(10));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during TMDb.GetMovieImagesAsync. MovieId: {id} Time: {time}", movieId, DateTime.UtcNow);
                return null;
            }
        }

        /// <summary>
        /// Consulta /configuration do TMDb (cache longa - até reinício).
        /// </summary>
        public async Task<ConfigurationDto?> GetConfigurationAsync()
        {
            const string cacheKey = "tmdb_configuration";
            if (_cache.TryGetValue(cacheKey, out ConfigurationDto cached))
            {
                _logger.LogInformation("TMDb Configuration cache HIT. Time: {time}", DateTime.UtcNow);
                return cached;
            }

            var client = CreateClient();
            var endpoint = BuildUrlWithKey("configuration");
            _logger.LogInformation("TMDb Configuration request. Endpoint: {endpoint} Time: {time}", endpoint, DateTime.UtcNow);

            try
            {
                var resp = await client.GetAsync(endpoint);
                var body = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation("TMDb Configuration response. StatusCode: {status} Time: {time}", (int)resp.StatusCode, DateTime.UtcNow);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("TMDb Configuration failed. Status: {status} Body: {body}", (int)resp.StatusCode, body);
                    return null;
                }

                var result = JsonSerializer.Deserialize<ConfigurationDto>(body, _jsonOptions);
                // cache longo (sem expiração absoluta; expira apenas ao reiniciar ou memória pressionada)
                _cache.Set(cacheKey, result!, new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.NeverRemove
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during TMDb.GetConfigurationAsync. Time: {time}", DateTime.UtcNow);
                return null;
            }
        }
    }
}
