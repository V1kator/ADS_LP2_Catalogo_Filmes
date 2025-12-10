FilmesApp

Este projeto é uma aplicação web desenvolvida em ASP.NET Core 9.0 que utiliza o SQLite como banco de dados e consome a API do The Movie Database (TMDb) para buscar informações sobre filmes.

O projeto foi desenvolvido utilizando o IDE JetBrains Rider.

Pré-requisitos

Para rodar esta aplicação, você precisará ter instalado:

1. NET 9.0 SDK: O SDK é necessário para compilar e executar a aplicação.
2. Um editor de código ou IDE:

• JetBrains Rider (Recomendado, pois o projeto foi desenvolvido nele).

• Visual Studio (com a workload de desenvolvimento web).

•
Visual Studio Code (com as extensões C# e ASP.NET Core).

Configuração do Projeto

1. Clonar o Repositório

git clone https://github.com/V1kator/ADS_LP2_Catalogo_Filmes

2. Configuração da API Key do TMDb

A aplicação utiliza a API do TMDb para buscar dados de filmes. Você precisará de uma chave de API válida.

1. Obtenha sua chave de API no site do The Movie Database (TMDb).

2. Edite o arquivo FilmesApp/FilmesApp/appsettings.Development.json.

3. Substitua o valor da chave TmdbApiKey pela sua chave real.

JSON

// FilmesApp/FilmesApp/appsettings.Development.json
{
"TmdbApiKey": "SUA_CHAVE_AQUI", // Substitua por sua chave real
"ConnectionStrings": {
"FilmeDb": "Data Source=Data/filmes.db"
},
// ... (restante do arquivo)
}

3. Configuração do Banco de Dados

O projeto utiliza SQLite e o arquivo do banco de dados (filmes.db) é criado automaticamente na primeira execução, se não existir, a partir do script db/create_database.sql.

•
Localização do Arquivo DB: O arquivo filmes.db será criado dentro da pasta FilmesApp/FilmesApp/Data/.

•
Script de Criação: O script db/create_database.sql contém a estrutura inicial da tabela Filmes.

Como Rodar a Aplicação

Você pode rodar a aplicação de duas maneiras: via linha de comando ou através do Rider/Visual Studio.

Opção 1: Linha de Comando (Terminal)

1. Navegue até o diretório do projeto que contém o arquivo .csproj:

Bash

cd FilmesApp/FilmesApp

2. Restaure as dependências do projeto:

3. Bash

dotnet restore

4. Execute a aplicação:

Bash

dotnet run

5. A aplicação será iniciada e o endereço (URL) será exibido no terminal (geralmente http://localhost:5000 ou http://localhost:5001 ).

Opção 2: JetBrains Rider ou Visual Studio

1. Abra o arquivo de solução (FilmesApp.sln) no Rider ou Visual Studio.

2. Certifique-se de que o projeto FilmesApp está selecionado como o projeto de inicialização.

3. Pressione o botão Run (Geralmente um triângulo verde) ou use o atalho (F5 no Visual Studio, Shift+F10 no Rider).

4. O IDE irá restaurar as dependências, compilar o projeto e abrir o navegador na URL da aplicação.

Observação: Se você estiver rodando em um ambiente de produção, certifique-se de configurar a TmdbApiKey como uma variável de ambiente ou em um arquivo de configuração seguro, e não diretamente no appsettings.json.

