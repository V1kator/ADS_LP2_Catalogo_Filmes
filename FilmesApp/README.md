# FilmesApp

Aplicação web desenvolvida em **ASP.NET Core 9.0**, utilizando
**SQLite** como banco de dados e integrando com a **API do The Movie
Database (TMDb)** para buscar informações sobre filmes.

O projeto foi desenvolvido utilizando o **JetBrains Rider**.

------------------------------------------------------------------------

## Pré-requisitos

Para rodar esta aplicação, é necessário ter instalado:

### **1. .NET 9.0 SDK**

Necessário para compilar e executar a aplicação.

### **2. Um editor/IDE**

-   **JetBrains Rider** (recomendado, projeto desenvolvido nele)\
-   **Visual Studio** (com a workload *Desenvolvimento Web*)\
-   **Visual Studio Code** (com extensões *C#* e *ASP.NET Core*)

------------------------------------------------------------------------

## ⚙️ Configuração do Projeto

### **1. Clonar o repositório**

``` bash
git clone https://github.com/V1kator/ADS_LP2_Catalogo_Filmes
```

------------------------------------------------------------------------

### **2. Configurar a API Key do TMDb**

A aplicação utiliza a API do TMDb para buscar dados de filmes.

#### **Passos:**

1.  Obtenha sua chave no site oficial do TMDb.\
2.  Edite o arquivo:

```{=html}
<!-- -->
```
    FilmesApp/FilmesApp/appsettings.Development.json

3.  Substitua o valor da chave:

``` json
{
  "TmdbApiKey": "SUA_CHAVE_AQUI",
  "ConnectionStrings": {
    "FilmeDb": "Data Source=Data/filmes.db"
  }
}
```

------------------------------------------------------------------------

### **3. Configuração do Banco de Dados**

O projeto utiliza **SQLite**.

-   O arquivo **filmes.db** será criado automaticamente na primeira
    execução, caso não exista.
-   O script opcional `db/create_database.sql` contém a estrutura
    inicial da tabela **Filmes**.

------------------------------------------------------------------------

## ▶ Como Rodar a Aplicação

Você pode rodar via terminal ou via IDE.

------------------------------------------------------------------------

### **Opção 1: Linha de Comando**

Navegue até a pasta do projeto:

``` bash
cd FilmesApp/FilmesApp
```

Restaure as dependências:

``` bash
dotnet restore
```

Execute:

``` bash
dotnet run
```

A aplicação iniciará em:

-   http://localhost:5000\
-   ou\
-   http://localhost:5001

------------------------------------------------------------------------

### **Opção 2: JetBrains Rider ou Visual Studio**

1.  Abra o arquivo **FilmesApp.sln**\
2.  Selecione o projeto *FilmesApp* como startup\
3.  Pressione **Run** (F5 no VS, Shift+F10 no Rider)

------------------------------------------------------------------------

## 🔐 Observação Importante

Para ambientes de produção, configure a **TmdbApiKey** como variável de
ambiente.\
Nunca deixe sua chave exposta no `appsettings.json`.

------------------------------------------------------------------------
