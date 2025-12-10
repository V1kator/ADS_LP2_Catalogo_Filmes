-- Script para criação do banco de dados SQLite e tabela Filmes (RF01)
DROP TABLE IF EXISTS Filmes;
CREATE TABLE Filmes (
    Id INTEGER PRIMARY KEY auto_increment,
    TmdbId INTEGER NOT NULL,
    Titulo TEXT NOT NULL,
    Sinopse TEXT,
    DataLancamento TEXT,
    IdiomaOriginal TEXT,
    Avaliacao REAL,
    PosterPath TEXT,
    Latitude REAL NOT NULL,
    Longitude REAL NOT NULL
);
