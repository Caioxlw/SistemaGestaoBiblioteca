using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace BibliotecaAPI.Data;

public class BibliotecaDbContext : DbContext
{
    public BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options) 
        : base(options) 
    { 
    }

    public DbSet<Autor> Autores { get; set; }
    public DbSet<Livro> Livros { get; set; }
    public DbSet<Aluno> Alunos { get; set; }
    public DbSet<Emprestimo> Emprestimos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<LogAuditoria> Auditoria { get; set; }
    public DbSet<Notificacao> Notificacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Aluno>()
            .HasIndex(a => a.Matricula)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Seed Users
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nome = "Administrador",
                Email = "admin@smartlib.com",
                SenhaHash = "$2a$11$ohkyeBgXwphJebuiUHq5fu0rtUJ8xrne4MQrc8Livs2HGW8tYRdv.",
                Perfil = PerfilUsuario.Admin
            },
            new Usuario
            {
                Id = 2,
                Nome = "Bibliotecário Chefe",
                Email = "biblio@smartlib.com",
                SenhaHash = "$2a$11$kaOr4lVi/FizMj1GwKQR2ujlE9nmbF65tQ/nX5zHgWZdeE3FHbCHy",
                Perfil = PerfilUsuario.Bibliotecario
            },
            new Usuario
            {
                Id = 3,
                Nome = "Aluno Exemplo",
                Email = "aluno@smartlib.com",
                SenhaHash = "$2a$11$QQfyZ8TbNMfc.QFLztUN7eqrUG7ICAv89ksSg77WE/Qh7d1FEf6si",
                Perfil = PerfilUsuario.Aluno,
                AlunoId = 1
            }
        );

        // Seed de um aluno para o usuario 3
        modelBuilder.Entity<Aluno>().HasData(
            new Aluno
            {
                Id = 1,
                Nome = "Aluno Exemplo",
                Email = "aluno@smartlib.com",
                Matricula = "2026001"
            }
        );

        // Seed Autor
        modelBuilder.Entity<Autor>().HasData(
            new Autor { Id = 1, Nome = "J.R.R. Tolkien", Nacionalidade = "Britânico", DataNascimento = new DateTime(1892, 1, 3, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed Livro (Sem Estoque para teste de reserva)
        modelBuilder.Entity<Livro>().HasData(
            new Livro { Id = 1, Isbn = "9780007136599", Titulo = "O Senhor dos Anéis", Descricao = "Fantasia épica.", AnoPublicacao = 1954, Editora = "HarperCollins", Categoria = "Fantasia", Quantidade = 0, Localizacao = "A1", AutorId = 1 },
            new Livro { Id = 2, Isbn = "9781234567897", Titulo = "Head First Java", Descricao = "Livro de java", AnoPublicacao = 2003, Editora = "OReilly", Categoria = "Tecnologia", Quantidade = 5, Localizacao = "T1", AutorId = 1 }
        );
    }
}