using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using FilmesApp.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace FilmesApp.Repositories
{
    // Implementação de IFilmeRepository usando SQLite
    public class FilmeRepository : IFilmeRepository
    {
        private readonly string _connectionString;

        public FilmeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("FilmeDb");
        }

        public Filme Create(Filme filme)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Filmes (TmdbId, Titulo, Sinopse, DataLancamento, IdiomaOriginal, Avaliacao, PosterPath, Latitude, Longitude)
                VALUES ($tmdbId, $titulo, $sinopse, $data, $idioma, $avaliacao, $poster, $lat, $lon);
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("$tmdbId", filme.TmdbId);
            cmd.Parameters.AddWithValue("$titulo", filme.Titulo);
            cmd.Parameters.AddWithValue("$sinopse", filme.Sinopse);
            cmd.Parameters.AddWithValue("$data", filme.DataLancamento.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$idioma", filme.IdiomaOriginal);
            cmd.Parameters.AddWithValue("$avaliacao", filme.Avaliacao);
            cmd.Parameters.AddWithValue("$poster", filme.PosterPath);
            cmd.Parameters.AddWithValue("$lat", filme.Latitude);
            cmd.Parameters.AddWithValue("$lon", filme.Longitude);

            var id = (long)cmd.ExecuteScalar();
            filme.Id = (int)id;
            return filme;
        }

        public IEnumerable<Filme> ReadAll()
        {
            var list = new List<Filme>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, TmdbId, Titulo, Sinopse, DataLancamento, IdiomaOriginal, Avaliacao, PosterPath, Latitude, Longitude FROM Filmes";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Filme
                {
                    Id = reader.GetInt32(0),
                    TmdbId = reader.GetInt32(1),
                    Titulo = reader.GetString(2),
                    Sinopse = reader.GetString(3),
                    DataLancamento = DateTime.Parse(reader.GetString(4)),
                    IdiomaOriginal = reader.GetString(5),
                    Avaliacao = reader.GetDouble(6),
                    PosterPath = reader.GetString(7),
                    Latitude = reader.GetDouble(8),
                    Longitude = reader.GetDouble(9)
                });
            }
            return list;
        }

        public Filme? GetById(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, TmdbId, Titulo, Sinopse, DataLancamento, IdiomaOriginal, Avaliacao, PosterPath, Latitude, Longitude 
                FROM Filmes WHERE Id = $id
            ";
            cmd.Parameters.AddWithValue("$id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Filme
                {
                    Id = reader.GetInt32(0),
                    TmdbId = reader.GetInt32(1),
                    Titulo = reader.GetString(2),
                    Sinopse = reader.GetString(3),
                    DataLancamento = DateTime.Parse(reader.GetString(4)),
                    IdiomaOriginal = reader.GetString(5),
                    Avaliacao = reader.GetDouble(6),
                    PosterPath = reader.GetString(7),
                    Latitude = reader.GetDouble(8),
                    Longitude = reader.GetDouble(9)
                };
            }
            return null;
        }

        public void Update(Filme filme)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Filmes SET 
                    TmdbId = $tmdbId, Titulo = $titulo, Sinopse = $sinopse, 
                    DataLancamento = $data, IdiomaOriginal = $idioma, Avaliacao = $avaliacao, PosterPath = $poster,
                    Latitude = $lat, Longitude = $lon
                WHERE Id = $id;
            ";
            cmd.Parameters.AddWithValue("$tmdbId", filme.TmdbId);
            cmd.Parameters.AddWithValue("$titulo", filme.Titulo);
            cmd.Parameters.AddWithValue("$sinopse", filme.Sinopse);
            cmd.Parameters.AddWithValue("$data", filme.DataLancamento.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$idioma", filme.IdiomaOriginal);
            cmd.Parameters.AddWithValue("$avaliacao", filme.Avaliacao);
            cmd.Parameters.AddWithValue("$poster", filme.PosterPath);
            cmd.Parameters.AddWithValue("$lat", filme.Latitude);
            cmd.Parameters.AddWithValue("$lon", filme.Longitude);
            cmd.Parameters.AddWithValue("$id", filme.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Filmes WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
