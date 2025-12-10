using FilmesApp.Models.TmdbDtos;
using System;

namespace FilmesApp.Utils
{
    /// <summary>
    /// Ajuda a montar a URL final do poster usando o resultado de /configuration.
    /// Se não houver configuração, retorna diretamente o poster_path (pode ser vazio).
    /// </summary>
    public static class PosterUrlBuilder
    {
        public static string BuildPosterUrl(ConfigurationDto? config, string? posterPath)
        {
            if (string.IsNullOrEmpty(posterPath)) return "/images/no-poster.png";

            if (config?.Images?.BaseUrl != null && config.Images.PosterSizes != null &&
                config.Images.PosterSizes.Count > 0)
            {
                // Escolhe tamanho médio: tenta "w342" ou pega o índice 2 se existir
                var size = config.Images.PosterSizes.Contains("w342") ? "w342" :
                    config.Images.PosterSizes.Count >= 3 ? config.Images.PosterSizes[2] :
                    config.Images.PosterSizes[0];

                // posterPath geralmente começa com '/'
                return $"{config.Images.BaseUrl}{size}{posterPath}";
            }

            // fallback: retorna apenas o path (não é URL completa)
            return posterPath;
        }
    }
}