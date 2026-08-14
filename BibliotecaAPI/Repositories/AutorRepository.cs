using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Repositories;

public class AutorRepository : IAutorRepository
{
    private readonly BibliotecaDbContext _context;

    public AutorRepository(BibliotecaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Autor>> ObterTodosAsync() =>
        await _context.Autores.ToListAsync();

    public async Task<Autor?> ObterPorIdAsync(int id) =>
        await _context.Autores.FindAsync(id);

    public async Task AdicionarAsync(Autor autor)
    {
        await _context.Autores.AddAsync(autor);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Autor autor)
    {
        _context.Autores.Update(autor);
        await _context.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var autor = await ObterPorIdAsync(id);
        if (autor != null)
        {
            _context.Autores.Remove(autor);
            await _context.SaveChangesAsync();
        }
    }
}