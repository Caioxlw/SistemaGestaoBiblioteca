using BibliotecaAPI.Models;

namespace BibliotecaAPI.Repositories;

public interface IEmprestimoRepository
{
    Task<Emprestimo?> ObterPorIdAsync(int id);
    Task<bool> PossuiEmprestimoAtivoAsync(int alunoId, int livroId);
    Task<IEnumerable<Emprestimo>> ObterTodosAsync();
    Task<IEnumerable<Emprestimo>> ObterAbertosAsync();
    Task<IEnumerable<Emprestimo>> ObterPorAlunoAsync(int alunoId);
    Task AdicionarAsync(Emprestimo emprestimo);
    Task AtualizarAsync(Emprestimo emprestimo);
}