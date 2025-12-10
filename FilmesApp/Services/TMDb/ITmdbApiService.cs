using System.Threading.Tasks;
using FilmesApp.Models.TmdbDtos;

namespace FilmesApp.Services.TMDb
{
    
    public interface ITmdbApiService
    {
        Task<SearchResultDto?> SearchMoviesAsync(string query, int page = 1);
        Task<MovieDetailsDto?> GetMovieDetailsAsync(int movieId);
        Task<MovieImagesDto?> GetMovieImagesAsync(int movieId);
        Task<ConfigurationDto?> GetConfigurationAsync();
    }
}