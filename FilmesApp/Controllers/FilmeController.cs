using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FilmesApp.DTOs;
using FilmesApp.Models;
using FilmesApp.Models.TmdbDtos;
using FilmesApp.Repositories;
using FilmesApp.Services.TMDb;
using FilmesApp.Services.Weather;
using FilmesApp.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FilmesApp.Controllers
{
    /// <summary>
    /// Controller principal para gerenciamento do catálogo de filmes.
    /// Contém Index, Search (TMDb), Import, Details (com previsão), Create/Edit/Delete, Export CSV/Excel.
    /// </summary>
    public class FilmeController : Controller
    {
        private readonly IFilmeRepository _repo;
        private readonly ITmdbApiService _tmdb;
        private readonly IWeatherApiService _weather;
        private readonly ILogger<FilmeController> _logger;

        public FilmeController(IFilmeRepository repo,
                               ITmdbApiService tmdb,
                               IWeatherApiService weather,
                               ILogger<FilmeController> logger)
        {
            _repo = repo;
            _tmdb = tmdb;
            _weather = weather;
            _logger = logger;
        }

        // Index: lista filmes locais
        public IActionResult Index()
        {
            var filmes = _repo.ReadAll().ToList();
            return View(filmes);
        }

        // Search: consulta TMDb (server-side pagination)
        public async Task<IActionResult> Search(string query = "", int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ViewBag.Message = "Digite um termo para buscar filmes no TMDb.";
                return View(null);
            }

            _logger.LogInformation("Search action called. Query: {q} Page: {p} Time: {t}", query, page,
                DateTime.UtcNow);
            var result = await _tmdb.SearchMoviesAsync(query, page);
            if (result == null)
            {
                ViewBag.Error = "Erro ao consultar TMDb. Verifique logs.";
                return View(null);
            }

            // Mapeia poster_path -> URL completa usando /configuration
            var config = await _tmdb.GetConfigurationAsync();
            if (result.Results != null && config != null)
            {
                foreach (var item in result.Results)
                {
                    // substitui o poster path relativo por URL completa (PosterUrlBuilder lida com nulls)
                    item.PosterPath = PosterUrlBuilder.BuildPosterUrl(config, item.PosterPath);
                }
            }

            ViewBag.Query = query;
            return View(result);
        }


        // Mostra detalhes (dados locais se importado, e detalhes do TMDb + previsão do tempo)
        public async Task<IActionResult> Details(int id, int? tmdbId = null)
        {
            // Prioriza registro local
            var filmeLocal = _repo.GetById(id);
            MovieDetailsDto? tmdbDetails = null;
            // ... código anterior que obtém tmdbDetails ...
            if (filmeLocal != null)
            {
                if (filmeLocal.TmdbId > 0)
                {
                    tmdbDetails = await _tmdb.GetMovieDetailsAsync(filmeLocal.TmdbId);
                }
            }
            else if (tmdbId.HasValue)
            {
                tmdbDetails = await _tmdb.GetMovieDetailsAsync(tmdbId.Value);
            }

// montar URL completa do poster para exibição (se necessário)
            if (tmdbDetails != null)
            {
                var config = await _tmdb.GetConfigurationAsync();
                if (config != null)
                {
                    tmdbDetails.PosterPath = PosterUrlBuilder.BuildPosterUrl(config, tmdbDetails.PosterPath);
                }
            }


            // Previsão do tempo: pega lat/long do local (obrigatório nos requisitos)
            double? lat = filmeLocal?.Latitude;
            double? lon = filmeLocal?.Longitude;
            if (lat == null || lon == null || (lat == 0 && lon == 0))
            {
                ViewBag.WeatherMessage = "Este filme não possui coordenadas. Importe ou edite o filme e forneça latitude/longitude.";
                return View((filmeLocal, tmdbDetails, (FilmesApp.Services.Weather.Models.WeatherForecastDto?)null));
            }

            var weather = await _weather.GetWeatherForecastAsync(lat.Value, lon.Value);
            if (weather == null)
            {
                ViewBag.WeatherMessage = "Não foi possível obter a previsão do tempo no momento.";
            }

            return View((filmeLocal, tmdbDetails, weather));
        }

        // Importa um filme selecionado do TMDb para o banco local
        [HttpPost]
        public async Task<IActionResult> Import(int tmdbId, double? latitude = null, double? longitude = null)
        {
            _logger.LogInformation("Import request. TMDbId: {id} Time: {t}", tmdbId, DateTime.UtcNow);

            var details = await _tmdb.GetMovieDetailsAsync(tmdbId);
            var config = await _tmdb.GetConfigurationAsync();

            if (details == null)
            {
                _logger.LogError("Import failed: TMDb details null for id {id}", tmdbId);
                TempData["Error"] = "Não foi possível obter detalhes do TMDb.";
                return RedirectToAction("Search");
            }

            // Tenta extrair lat/long do TMDb - TMDb não fornece coordenadas de filme normalmente.
            // Aqui deixamos o valor vindo do parâmetro ou default 0 => usuário precisa preencher depois.
            var filme = new Filme
            {
                TmdbId = details.Id,
                Titulo = details.Title ?? "Sem título",
                Sinopse = details.Overview ?? string.Empty,
                DataLancamento = DateTime.TryParse(details.ReleaseDate, out var dt) ? dt : DateTime.MinValue,
                IdiomaOriginal = details.OriginalLanguage ?? string.Empty,
                Avaliacao = details.VoteAverage,
                PosterPath = PosterUrlBuilder.BuildPosterUrl(config, details.PosterPath),
                Latitude = latitude ?? 0,
                Longitude = longitude ?? 0
            };

            var created = _repo.Create(filme);
            TempData["Success"] = $"Filme '{created.Titulo}' importado com sucesso (Id local: {created.Id}).";
            return RedirectToAction("Index");
        }

        // Create - GET
        public IActionResult Create()
        {
            return View(new FilmeCreateDto { DataLancamento = DateTime.Today });
        }

        // Create - POST
        [HttpPost]
        public IActionResult Create(FilmeCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var filme = new Filme
            {
                TmdbId = 0,
                Titulo = dto.Titulo,
                Sinopse = dto.Sinopse,
                DataLancamento = dto.DataLancamento,
                IdiomaOriginal = dto.IdiomaOriginal,
                Avaliacao = dto.Avaliacao,
                PosterPath = dto.PosterPath,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };

            _repo.Create(filme);
            TempData["Success"] = "Filme criado com sucesso.";
            return RedirectToAction("Index");
        }

        // Edit - GET
        public IActionResult Edit(int id)
        {
            var filme = _repo.GetById(id);
            if (filme == null) return NotFound();
            var dto = new FilmeEditDto
            {
                Id = filme.Id,
                Titulo = filme.Titulo,
                Sinopse = filme.Sinopse,
                DataLancamento = filme.DataLancamento,
                IdiomaOriginal = filme.IdiomaOriginal,
                Avaliacao = filme.Avaliacao,
                PosterPath = filme.PosterPath,
                Latitude = filme.Latitude,
                Longitude = filme.Longitude
            };
            return View(dto);
        }

        // Edit - POST
        [HttpPost]
        public IActionResult Edit(FilmeEditDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var existing = _repo.GetById(dto.Id);
            if (existing == null) return NotFound();

            existing.Titulo = dto.Titulo;
            existing.Sinopse = dto.Sinopse;
            existing.DataLancamento = dto.DataLancamento;
            existing.IdiomaOriginal = dto.IdiomaOriginal;
            existing.Avaliacao = dto.Avaliacao;
            existing.PosterPath = dto.PosterPath;
            existing.Latitude = dto.Latitude;
            existing.Longitude = dto.Longitude;

            _repo.Update(existing);
            TempData["Success"] = "Filme atualizado com sucesso.";
            return RedirectToAction("Index");
        }

        // Delete - GET
        public IActionResult Delete(int id)
        {
            var filme = _repo.GetById(id);
            if (filme == null) return NotFound();
            return View(filme);
        }

        // Delete - POST
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _repo.Delete(id);
            TempData["Success"] = "Filme removido.";
            return RedirectToAction("Index");
        }

        // Export CSV
        public IActionResult ExportCsv()
        {
            var filmes = _repo.ReadAll().ToList();
            var csvBytes = CsvExcelExporter.ExportToCsvBytes(filmes);
            return File(csvBytes, "text/csv", $"catalogo_filmes_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }

        // Export Excel
        public IActionResult ExportExcel()
        {
            var filmes = _repo.ReadAll().ToList();
            var excelBytes = CsvExcelExporter.ExportToExcelBytes(filmes);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"catalogo_filmes_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
        }
    }
}
