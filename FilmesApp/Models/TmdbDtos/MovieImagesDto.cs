using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FilmesApp.Models.TmdbDtos
{
    // DTO simples para resposta de /movie/{id}/images
    public class MovieImagesDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }

        [JsonPropertyName("backdrops")] public List<ImageItem>? Backdrops { get; set; }

        [JsonPropertyName("posters")] public List<ImageItem>? Posters { get; set; }
    }

    public class ImageItem
    {
        [JsonPropertyName("file_path")] public string? FilePath { get; set; }

        [JsonPropertyName("width")] public int? Width { get; set; }

        [JsonPropertyName("height")] public int? Height { get; set; }

        [JsonPropertyName("iso_639_1")] public string? Language { get; set; }
    }
}