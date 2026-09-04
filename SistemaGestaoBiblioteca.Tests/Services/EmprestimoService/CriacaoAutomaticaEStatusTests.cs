using BibliotecaAPI.DTOs;
using BibliotecaAPI.Models;
using BibliotecaAPI.Services;
using Moq;
using Xunit;

namespace SistemaGestaoBiblioteca.Tests.Services.EmprestimoServiceTests;

public class CriacaoAutomaticaEStatusTests
{
    [Fact]
    public async Task DeveDefinirDataPrevistaAutomaticamenteECalcularAtrasoNaConsulta()
    {
        var alunoRepo = new Mock<BibliotecaAPI.Repositories.IAlunoRepository>();
        alunoRepo.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(new Aluno { Id = 1, Nome = "Aluno" });

        var livroRepo = new Mock<BibliotecaAPI.Repositories.ILivroRepository>();
        livroRepo.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(new Livro { Id = 1, Titulo = "Livro", Quantidade = 2 });

        var emprestimoRepo = new Mock<BibliotecaAPI.Repositories.IEmprestimoRepository>();
        emprestimoRepo.Setup(r => r.PossuiEmprestimoAtivoAsync(1, 1)).ReturnsAsync(false);
        emprestimoRepo.Setup(r => r.AdicionarAsync(It.IsAny<Emprestimo>())).Returns(Task.CompletedTask);
        emprestimoRepo.Setup(r => r.ObterTodosAsync()).ReturnsAsync(Array.Empty<Emprestimo>());

        var service = new EmprestimoService(emprestimoRepo.Object, livroRepo.Object, alunoRepo.Object);

        var criado = await service.CriarAsync(new CriarEmprestimoDto
        {
            AlunoId = 1,
            LivroId = 1
        });

        Assert.Equal(14, (criado.DataPrevistaDevolucao.Date - criado.DataEmprestimo.Date).Days);
        Assert.Equal("Ativo", criado.Status);

        var emprestimoAtrasado = new Emprestimo
        {
            Id = 2,
            AlunoId = 1,
            LivroId = 1,
            DataEmprestimo = DateTime.UtcNow.Date.AddDays(-20),
            DataPrevistaDevolucao = DateTime.UtcNow.Date.AddDays(-3),
            Status = StatusEmprestimo.Ativo,
            Aluno = new Aluno { Id = 1, Nome = "Aluno" },
            Livro = new Livro { Id = 1, Titulo = "Livro", Quantidade = 2 }
        };

        emprestimoRepo.Setup(r => r.ObterTodosAsync()).ReturnsAsync(new[] { emprestimoAtrasado });

        var listagem = await service.ObterTodosAsync();
        var item = Assert.Single(listagem);

        Assert.Equal("Atrasado", item.Status);
        Assert.Equal(3, item.DiasAtraso);
        Assert.Equal(6.00m, item.Multa);
    }
}