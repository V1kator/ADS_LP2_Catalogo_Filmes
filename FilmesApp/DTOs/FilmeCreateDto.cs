using System;
using System.ComponentModel.DataAnnotations;

namespace FilmesApp.DTOs
{
    // DTO para criação de filme manualmente
    public class FilmeCreateDto
    {
        [Required] public string Titulo { get; set; } = string.Empty;
        [Required] public string Sinopse { get; set; } = string.Empty;
        [Required] public DateTime DataLancamento { get; set; }
        [Required] public string IdiomaOriginal { get; set; } = string.Empty;
        public double Avaliacao { get; set; }
        public string PosterPath { get; set; } = string.Empty;
        [Required] public double Latitude { get; set; }
        [Required] public double Longitude { get; set; }
    }
}