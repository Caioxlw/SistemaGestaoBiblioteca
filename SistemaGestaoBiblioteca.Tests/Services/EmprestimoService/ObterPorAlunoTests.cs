using Xunit;
using Moq;
using BibliotecaAPI.Services;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaGestaoBiblioteca.Tests.Services.EmprestimoServiceTests;

public class ObterPorAlunoTests
{
    [Fact]
    public async Task ObterPorAlunoAsync_DeveRetornarEmprestimosComDiasAtrasoEMulta()
    {
        // Arrange
        var mockEmprestimoRepo = new Mock<IEmprestimoRepository>();
        var mockLivroRepo = new Mock<ILivroRepository>();
        var mockAlunoRepo = new Mock<IAlunoRepository>();

        int alunoId = 1;
        var emprestimos = new List<Emprestimo>
        {
            new Emprestimo
            {
                Id = 1,
                AlunoId = alunoId,
                LivroId = 10,
                DataEmprestimo = DateTime.UtcNow.AddDays(-20),
                DataPrevistaDevolucao = DateTime.UtcNow.AddDays(-6), // 6 dias de atraso
                Status = StatusEmprestimo.Ativo,
                Livro = new Livro { Id = 10, Titulo = "Livro Atrasado" },
                Aluno = new Aluno { Id = alunoId, Nome = "Aluno Teste" }
            },
            new Emprestimo
            {
                Id = 2,
                AlunoId = alunoId,
                LivroId = 20,
                DataEmprestimo = DateTime.UtcNow.AddDays(-2),
                DataPrevistaDevolucao = DateTime.UtcNow.AddDays(12), // em dia
                Status = StatusEmprestimo.Ativo,
                Livro = new Livro { Id = 20, Titulo = "Livro Em Dia" },
                Aluno = new Aluno { Id = alunoId, Nome = "Aluno Teste" }
            }
        };

        mockEmprestimoRepo.Setup(r => r.ObterPorAlunoAsync(alunoId)).ReturnsAsync(emprestimos);

        var service = new EmprestimoService(
            mockEmprestimoRepo.Object,
            mockLivroRepo.Object,
            mockAlunoRepo.Object
        );

        // Act
        var resultado = (await service.ObterPorAlunoAsync(alunoId)).ToList();

        // Assert
        Assert.Equal(2, resultado.Count);

        var itemAtrasado = resultado.First(e => e.Id == 1);
        Assert.True(itemAtrasado.DiasAtraso >= 6);
        Assert.True(itemAtrasado.Multa >= 12.00m);
        Assert.Equal("Livro Atrasado", itemAtrasado.TituloLivro);

        var itemEmDia = resultado.First(e => e.Id == 2);
        Assert.Equal(0, itemEmDia.DiasAtraso);
        Assert.Equal(0, itemEmDia.Multa);
        Assert.Equal("Livro Em Dia", itemEmDia.TituloLivro);
    }
}
