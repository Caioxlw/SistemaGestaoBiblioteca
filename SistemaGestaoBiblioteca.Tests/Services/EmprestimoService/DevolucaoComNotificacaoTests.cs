using Xunit;
using Moq;
using BibliotecaAPI.Services;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;
using System;
using System.Threading.Tasks;

namespace SistemaGestaoBiblioteca.Tests.Services.EmprestimoServiceTests;

public class DevolucaoComNotificacaoTests
{
    [Fact]
    public async Task DevolverAsync_DeveDispararNotificacaoParaProximoDaFila()
    {
        // Arrange
        var mockEmprestimoRepo = new Mock<IEmprestimoRepository>();
        var mockLivroRepo = new Mock<ILivroRepository>();
        var mockNotificationService = new Mock<INotificationService>();

        var emprestimo = new Emprestimo
        {
            Id = 10,
            LivroId = 42,
            AlunoId = 1,
            Status = StatusEmprestimo.Ativo
        };

        var livro = new Livro
        {
            Id = 42,
            Titulo = "Clean Code",
            Quantidade = 0
        };

        mockEmprestimoRepo.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(emprestimo);
        mockLivroRepo.Setup(r => r.ObterPorIdAsync(42)).ReturnsAsync(livro);

        var service = new EmprestimoService(
            mockEmprestimoRepo.Object,
            mockLivroRepo.Object,
            null!,
            mockNotificationService.Object
        );

        // Act
        var resultado = await service.DevolverAsync(10);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Devolvido", resultado.Status);
        Assert.Equal(1, livro.Quantidade);

        // Verifica que a notificação foi chamada exatamente 1 vez com o ID do livro
        mockNotificationService.Verify(n => n.NotificarProximoDaFilaAsync(42), Times.Once);
    }
}
