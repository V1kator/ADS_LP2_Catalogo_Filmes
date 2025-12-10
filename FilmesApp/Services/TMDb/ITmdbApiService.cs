using System.Threading.Tasks;
using FilmesApp.Models.TmdbDtos;

namespace FilmesApp.Services.TMDb
{
    /// <summary>
    /// Interface do cliente TMDb desacoplado (RF05).
    /// Define os métodos obrigatórios: SearchMoviesAsync, GetMovieDetailsAsync, GetMovieImagesAsync, GetConfigurationAsync.
    /// </summary>
    public interface ITmdbApiService
    {
        Task<SearchResultDto?> SearchMoviesAsync(string query, int page = 1);
        Task<MovieDetailsDto?> GetMovieDetailsAsync(int movieId);
        Task<MovieImagesDto?> GetMovieImagesAsync(int movieId);
        Task<ConfigurationDto?> GetConfigurationAsync();
    }
}