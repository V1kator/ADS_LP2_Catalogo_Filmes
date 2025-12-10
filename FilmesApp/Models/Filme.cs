using System;

namespace FilmesApp.Models
{
    // Entidade Filme com todos os campos (RF01)
    public class Filme
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Sinopse { get; set; } = string.Empty;
        public DateTime DataLancamento { get; set; }
        public string IdiomaOriginal { get; set; } = string.Empty;
        public double Avaliacao { get; set; }
        public string PosterPath { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}