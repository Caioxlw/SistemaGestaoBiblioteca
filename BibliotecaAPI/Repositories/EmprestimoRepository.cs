using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Repositories;

public class EmprestimoRepository : IEmprestimoRepository
{
    private readonly BibliotecaDbContext _context;

    public EmprestimoRepository(BibliotecaDbContext context)
    {
        _context = context;
    }

    public async Task<Emprestimo?> ObterPorIdAsync(int id) =>
        await _context.Emprestimos
            .Include(e => e.Aluno)
            .Include(e => e.Livro)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<bool> PossuiEmprestimoAtivoAsync(int alunoId, int livroId) =>
        await _context.Emprestimos
            .AnyAsync(e => e.AlunoId == alunoId && 
                           e.LivroId == livroId && 
                           e.Status == StatusEmprestimo.Ativo);

    public async Task AdicionarAsync(Emprestimo emprestimo)
    {
        await _context.Emprestimos.AddAsync(emprestimo);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Emprestimo emprestimo)
    {
        _context.Emprestimos.Update(emprestimo);
        await _context.SaveChangesAsync();
    }
}