using Xunit;
using Moq;
using BibliotecaAPI.Services;
using BibliotecaAPI.Models;
using BibliotecaAPI.Exceptions;
using System.Threading.Tasks;

namespace SistemaGestaoBiblioteca.Tests.Services.EmprestimoServiceTests;

public class DevolucaoDuplicadaTests
{
    [Fact]
    public async Task DeveImpedirDevolucaoDuplicada()
    {
        // Arrange: Empréstimo já está com status Devolvido
        var mockEmprestimoRepo = new Moq.Mock<BibliotecaAPI.Repositories.IEmprestimoRepository>();
        var emprestimo = new Emprestimo { Id = 1, Status = StatusEmprestimo.Devolvido };
        mockEmprestimoRepo.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(emprestimo);

        var service = new EmprestimoService(mockEmprestimoRepo.Object, null!, null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() => service.DevolverAsync(1));
        Assert.Equal("Este empréstimo já foi devolvido.", ex.Message);
    }
}
