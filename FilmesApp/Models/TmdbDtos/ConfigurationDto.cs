using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FilmesApp.Models.TmdbDtos
{
    
    public class ConfigurationDto
    {
        [JsonPropertyName("images")] public ImagesConfig? Images { get; set; }
    }

    public class ImagesConfig
    {
        [JsonPropertyName("base_url")] public string? BaseUrl { get; set; }

        [JsonPropertyName("poster_sizes")] public List<string>? PosterSizes { get; set; }
    }
}