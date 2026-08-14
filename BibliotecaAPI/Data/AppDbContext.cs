using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Data;

public class BibliotecaDbContext : DbContext
{
    public BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options) 
        : base(options) 
    { 
    }

    // Representação das tabelas no banco de dados
    public DbSet<Autor> Autores { get; set; }
    public DbSet<Livro> Livros { get; set; }
    public DbSet<Aluno> Alunos { get; set; }
    public DbSet<Emprestimo> Emprestimos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Garantindo que a Matrícula seja única no banco de dados
        modelBuilder.Entity<Aluno>()
            .HasIndex(a => a.Matricula)
            .IsUnique();
    }
}