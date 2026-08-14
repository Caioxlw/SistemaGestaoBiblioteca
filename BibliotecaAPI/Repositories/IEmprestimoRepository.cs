using BibliotecaAPI.Models;

namespace BibliotecaAPI.Repositories;

public interface IEmprestimoRepository
{
    Task<Emprestimo?> ObterPorIdAsync(int id);
    Task<bool> PossuiEmprestimoAtivoAsync(int alunoId, int livroId);
    Task AdicionarAsync(Emprestimo emprestimo);
    Task AtualizarAsync(Emprestimo emprestimo);
}