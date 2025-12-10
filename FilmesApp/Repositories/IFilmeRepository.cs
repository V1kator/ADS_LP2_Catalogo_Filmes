using FilmesApp.Models;
using System.Collections.Generic;

namespace FilmesApp.Repositories
{
    // Interface para operações CRUD de Filmes
    public interface IFilmeRepository
    {
        Filme Create(Filme filme);
        IEnumerable<Filme> ReadAll();
        Filme? GetById(int id);
        void Update(Filme filme);
        void Delete(int id);
    }
}