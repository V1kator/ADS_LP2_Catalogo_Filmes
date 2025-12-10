using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FilmesApp.Services.Weather.Models
{
    // Modelagem simples do JSON retornado pelo Open-Meteo para daily temps
    public class WeatherForecastDto
    {
        [JsonPropertyName("latitude")] public double Latitude { get; set; }

        [JsonPropertyName("longitude")] public double Longitude { get; set; }

        [JsonPropertyName("generationtime_ms")]
        public double GenerationTimeMs { get; set; }

        [JsonPropertyName("utc_offset_seconds")]
        public int UtcOffsetSeconds { get; set; }

        [JsonPropertyName("timezone")] public string? Timezone { get; set; }

        [JsonPropertyName("daily")] public Daily? Daily { get; set; }
    }

    public class Daily
    {
        [JsonPropertyName("time")] public List<string>? Time { get; set; }

        [JsonPropertyName("temperature_2m_max")]
        public List<double>? TemperatureMax { get; set; }

        [JsonPropertyName("temperature_2m_min")]
        public List<double>? TemperatureMin { get; set; }
    }
}