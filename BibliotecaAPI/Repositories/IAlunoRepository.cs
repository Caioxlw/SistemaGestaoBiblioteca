using BibliotecaAPI.Models;

namespace BibliotecaAPI.Repositories;

public interface IAlunoRepository
{
    Task<Aluno?> ObterPorIdAsync(int id);
    Task<bool> ExisteMatriculaAsync(string matricula);
    Task AdicionarAsync(Aluno aluno);
}