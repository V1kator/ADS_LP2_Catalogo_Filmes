using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FilmesApp.Models.TmdbDtos
{
    // DTO para resultado da busca /search/movie
    public class SearchResultDto
    {
        [JsonPropertyName("page")] public int Page { get; set; }

        [JsonPropertyName("results")] public List<MovieItemDto>? Results { get; set; }

        [JsonPropertyName("total_results")] public int TotalResults { get; set; }

        [JsonPropertyName("total_pages")] public int TotalPages { get; set; }
    }

    // DTO interno para cada filme nos resultados
    public class MovieItemDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }

        [JsonPropertyName("title")] public string? Title { get; set; }

        [JsonPropertyName("original_language")]
        public string? OriginalLanguage { get; set; }

        [JsonPropertyName("overview")] public string? Overview { get; set; }

        [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }

        [JsonPropertyName("vote_average")] public double VoteAverage { get; set; }

        [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
    }
}