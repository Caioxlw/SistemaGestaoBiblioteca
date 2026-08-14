using BibliotecaAPI.Models;

namespace BibliotecaAPI.Repositories;

public interface ILivroRepository
{
    Task<IEnumerable<Livro>> ObterTodosAsync(string? titulo, string? autor);
    Task<Livro?> ObterPorIdAsync(int id);
    Task AdicionarAsync(Livro livro);
    Task AtualizarAsync(Livro livro);
}