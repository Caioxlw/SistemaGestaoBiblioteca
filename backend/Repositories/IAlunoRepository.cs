using BibliotecaAPI.Models;

namespace BibliotecaAPI.Repositories;

public interface IAlunoRepository
{
    Task<Aluno?> ObterPorIdAsync(int id);
    Task<IEnumerable<Aluno>> ObterTodosAsync();
    Task<bool> ExisteMatriculaAsync(string matricula);
    Task AdicionarAsync(Aluno aluno);
    Task AtualizarAsync(Aluno aluno);
    Task ExcluirAsync(int id);
}