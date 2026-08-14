using BibliotecaAPI.Models;

namespace BibliotecaAPI.Repositories;

public interface IAutorRepository
{
    Task<IEnumerable<Autor>> ObterTodosAsync();
    Task<Autor?> ObterPorIdAsync(int id);
    Task AdicionarAsync(Autor autor);
}