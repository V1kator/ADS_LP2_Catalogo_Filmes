using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FilmesApp.Services.Weather.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FilmesApp.Services.Weather
{
    /// <summary>
    /// Implementação do IWeatherApiService consumindo Open-Meteo.
    /// Endpoint: /v1/forecast?latitude={lat}&longitude={lon}&daily=temperature_2m_max,temperature_2m_min&timezone=auto
    /// Cache TTL = 10 minutos.
    /// </summary>
    public class WeatherApiService : IWeatherApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<WeatherApiService> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public WeatherApiService(IHttpClientFactory httpClientFactory,
                                 IMemoryCache cache,
                                 ILogger<WeatherApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
        }

        private HttpClient CreateClient() => _httpClientFactory.CreateClient("open-meteo");

        public async Task<WeatherForecastDto?> GetWeatherForecastAsync(double latitude, double longitude)
        {
            string cacheKey = $"weather::{latitude:F6}::{longitude:F6}";
            if (_cache.TryGetValue(cacheKey, out WeatherForecastDto cached))
            {
                _logger.LogInformation("Weather cache HIT. Lat: {lat} Lon: {lon} Time: {time}", latitude, longitude, DateTime.UtcNow);
                return cached;
            }

            var client = CreateClient();
            var endpoint = $"v1/forecast?latitude={latitude}&longitude={longitude}&daily=temperature_2m_max,temperature_2m_min&timezone=auto";
            _logger.LogInformation("Open-Meteo request. Endpoint: {endpoint} Lat: {lat} Lon: {lon} Time: {time}", endpoint, latitude, longitude, DateTime.UtcNow);

            try
            {
                var resp = await client.GetAsync(endpoint);
                var body = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation("Open-Meteo response. StatusCode: {status} Lat: {lat} Lon: {lon} Time: {time}", (int)resp.StatusCode, latitude, longitude, DateTime.UtcNow);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("Open-Meteo failed. Status: {status} Body: {body}", (int)resp.StatusCode, body);
                    return null;
                }

                var result = JsonSerializer.Deserialize<WeatherForecastDto>(body, _jsonOptions);
                // cache por 10 minutos
                _cache.Set(cacheKey, result!, TimeSpan.FromMinutes(10));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during WeatherApiService.GetWeatherForecastAsync. Lat: {lat} Lon: {lon} Time: {time}", latitude, longitude, DateTime.UtcNow);
                return null;
            }
        }
    }
}
