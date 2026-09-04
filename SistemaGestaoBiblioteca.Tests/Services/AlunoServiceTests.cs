using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;
using BibliotecaAPI.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SistemaGestaoBiblioteca.Tests.Services;

public class AlunoServiceTests
{
    private BibliotecaDbContext CriarContextoEmMemoria()
    {
        var options = new DbContextOptionsBuilder<BibliotecaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BibliotecaDbContext(options);
    }

    [Fact]
    public async Task CriarAsync_ComDadosValidos_DeveCriarAlunoEUsuarioVinculadoComSenhaPadrao()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var repo = new AlunoRepository(context);
        var mockAuditoria = new Mock<IAuditoriaService>();
        var service = new AlunoService(repo, context, mockAuditoria.Object);

        var dto = new CriarAlunoDto
        {
            Nome = "Lucas Pereira",
            Matricula = "2026100",
            Email = "lucas.pereira@smartlib.com"
        };

        // Act
        var resultado = await service.CriarAsync(dto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Lucas Pereira", resultado.Nome);
        Assert.Equal("2026100", resultado.Matricula);
        Assert.Equal("lucas.pereira@smartlib.com", resultado.Email);

        // Verifica que o aluno foi salvo no banco
        var alunoNoBanco = await context.Alunos.FirstOrDefaultAsync(a => a.Matricula == "2026100");
        Assert.NotNull(alunoNoBanco);

        // Verifica que o usuário vinculado foi criado automaticamente com Perfil Aluno e senha correta
        var usuarioNoBanco = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "lucas.pereira@smartlib.com");
        Assert.NotNull(usuarioNoBanco);
        Assert.Equal(PerfilUsuario.Aluno, usuarioNoBanco.Perfil);
        Assert.Equal(alunoNoBanco.Id, usuarioNoBanco.AlunoId);
        Assert.True(BCrypt.Net.BCrypt.Verify("Aluno@123", usuarioNoBanco.SenhaHash));
    }

    [Fact]
    public async Task CriarAsync_ComMatriculaDuplicada_DeveLancarConflictException()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        context.Alunos.Add(new Aluno { Nome = "Aluno 1", Matricula = "2026001", Email = "a1@test.com" });
        await context.SaveChangesAsync();

        var repo = new AlunoRepository(context);
        var service = new AlunoService(repo, context);

        var dto = new CriarAlunoDto
        {
            Nome = "Aluno 2",
            Matricula = "2026001",
            Email = "a2@test.com"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() => service.CriarAsync(dto));
        Assert.Contains("matrícula", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CriarAsync_ComEmailDuplicado_DeveLancarConflictException()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        context.Usuarios.Add(new Usuario
        {
            Nome = "Admin",
            Email = "admin@smartlib.com",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Admin
        });
        await context.SaveChangesAsync();

        var repo = new AlunoRepository(context);
        var service = new AlunoService(repo, context);

        var dto = new CriarAlunoDto
        {
            Nome = "Aluno Teste",
            Matricula = "2026099",
            Email = "admin@smartlib.com"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() => service.CriarAsync(dto));
        Assert.Contains("e-mail", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AtualizarAsync_ComDadosValidos_DeveAtualizarAlunoEUsuario()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var aluno = new Aluno { Nome = "Antigo", Matricula = "2026010", Email = "antigo@smartlib.com" };
        context.Alunos.Add(aluno);
        await context.SaveChangesAsync();

        var usuario = new Usuario
        {
            Nome = "Antigo",
            Email = "antigo@smartlib.com",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Aluno,
            AlunoId = aluno.Id
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var repo = new AlunoRepository(context);
        var service = new AlunoService(repo, context);

        var dto = new CriarAlunoDto
        {
            Nome = "Novo Nome",
            Matricula = "2026010",
            Email = "novo.email@smartlib.com"
        };

        // Act
        var resultado = await service.AtualizarAsync(aluno.Id, dto);

        // Assert
        Assert.Equal("Novo Nome", resultado.Nome);
        Assert.Equal("novo.email@smartlib.com", resultado.Email);

        var usuarioAtualizado = await context.Usuarios.FirstOrDefaultAsync(u => u.AlunoId == aluno.Id);
        Assert.NotNull(usuarioAtualizado);
        Assert.Equal("Novo Nome", usuarioAtualizado.Nome);
        Assert.Equal("novo.email@smartlib.com", usuarioAtualizado.Email);
    }

    [Fact]
    public async Task ExcluirAsync_ComEmprestimosPendentes_DeveLancarConflictException()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var aluno = new Aluno { Nome = "Devedor", Matricula = "2026015", Email = "devedor@smartlib.com" };
        context.Alunos.Add(aluno);
        await context.SaveChangesAsync();

        context.Emprestimos.Add(new Emprestimo
        {
            AlunoId = aluno.Id,
            LivroId = 1,
            DataEmprestimo = DateTime.UtcNow,
            DataPrevistaDevolucao = DateTime.UtcNow.AddDays(7),
            Status = StatusEmprestimo.Ativo
        });
        await context.SaveChangesAsync();

        var repo = new AlunoRepository(context);
        var service = new AlunoService(repo, context);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() => service.ExcluirAsync(aluno.Id));
        Assert.Contains("empréstimos pendentes", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExcluirAsync_SemPendencias_DeveExcluirAlunoEUsuario()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var aluno = new Aluno { Nome = "Aluno Livre", Matricula = "2026020", Email = "livre@smartlib.com" };
        context.Alunos.Add(aluno);
        await context.SaveChangesAsync();

        var usuario = new Usuario
        {
            Nome = "Aluno Livre",
            Email = "livre@smartlib.com",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Aluno,
            AlunoId = aluno.Id
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var repo = new AlunoRepository(context);
        var service = new AlunoService(repo, context);

        // Act
        await service.ExcluirAsync(aluno.Id);

        // Assert
        Assert.Null(await context.Alunos.FindAsync(aluno.Id));
        Assert.Null(await context.Usuarios.FirstOrDefaultAsync(u => u.AlunoId == aluno.Id));
    }
}
