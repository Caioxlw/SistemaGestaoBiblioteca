using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Repositories;

public class AlunoRepository : IAlunoRepository
{
    private readonly BibliotecaDbContext _context;

    public AlunoRepository(BibliotecaDbContext context)
    {
        _context = context;
    }

    public async Task<Aluno?> ObterPorIdAsync(int id) =>
        await _context.Alunos.FindAsync(id);

    public async Task<IEnumerable<Aluno>> ObterTodosAsync() =>
        await _context.Alunos.OrderBy(a => a.Nome).ToListAsync();

    public async Task<bool> ExisteMatriculaAsync(string matricula, int? ignorarId = null) =>
        ignorarId.HasValue
            ? await _context.Alunos.AnyAsync(a => a.Matricula == matricula && a.Id != ignorarId.Value)
            : await _context.Alunos.AnyAsync(a => a.Matricula == matricula);

    public async Task<bool> ExisteEmailAsync(string email, int? ignorarId = null) =>
        ignorarId.HasValue
            ? await _context.Alunos.AnyAsync(a => a.Email.ToLower() == email.ToLower() && a.Id != ignorarId.Value)
            : await _context.Alunos.AnyAsync(a => a.Email.ToLower() == email.ToLower());

    public async Task AdicionarAsync(Aluno aluno)
    {
        await _context.Alunos.AddAsync(aluno);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Aluno aluno)
    {
        _context.Alunos.Update(aluno);
        await _context.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var aluno = await ObterPorIdAsync(id);
        if (aluno != null)
        {
            _context.Alunos.Remove(aluno);
            await _context.SaveChangesAsync();
        }
    }
}