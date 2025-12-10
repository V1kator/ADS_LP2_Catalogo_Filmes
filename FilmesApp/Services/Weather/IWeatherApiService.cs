using System.Threading.Tasks;
using FilmesApp.Services.Weather.Models;

namespace FilmesApp.Services.Weather
{
    /// <summary>
    /// Interface do serviço de previsão do tempo (Open-Meteo).
    /// </summary>
    public interface IWeatherApiService
    {
        /// <summary>
        /// Obtém previsões diárias (min/max) para a latitude/longitude informada.
        /// Retorna null em caso de erro.
        /// </summary>
        Task<WeatherForecastDto?> GetWeatherForecastAsync(double latitude, double longitude);
    }
}