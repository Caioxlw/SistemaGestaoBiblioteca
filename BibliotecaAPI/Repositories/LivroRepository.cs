using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Repositories;

public class LivroRepository : ILivroRepository
{
    private readonly BibliotecaDbContext _context;

    public LivroRepository(BibliotecaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Livro>> ObterTodosAsync(string? titulo, string? autor)
    {
        var query = _context.Livros.Include(l => l.Autor).AsQueryable();

        if (!string.IsNullOrWhiteSpace(titulo))
            query = query.Where(l => l.Titulo.ToLower().Contains(titulo.ToLower()));

        if (!string.IsNullOrWhiteSpace(autor))
            query = query.Where(l => l.Autor != null && l.Autor.Nome.ToLower().Contains(autor.ToLower()));

        return await query.ToListAsync();
    }

    public async Task<Livro?> ObterPorIdAsync(int id) =>
        await _context.Livros.Include(l => l.Autor).FirstOrDefaultAsync(l => l.Id == id);

    public async Task AdicionarAsync(Livro livro)
    {
        await _context.Livros.AddAsync(livro);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Livro livro)
    {
        _context.Livros.Update(livro);
        await _context.SaveChangesAsync();
    }
}