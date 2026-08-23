using Xunit;
using Moq;
using BibliotecaAPI.Services;
using BibliotecaAPI.Models;
using BibliotecaAPI.Exceptions;
using System.Threading.Tasks;

namespace SistemaGestaoBiblioteca.Tests.Services.EmprestimoServiceTests;

public class EmprestimoDuplicadoTests
{
    [Fact]
    public async Task DeveImpedirEmprestimoDuplicado()
    {
        // Arrange: Configurar mock para retornar verdadeiro no PossuiEmprestimoAtivoAsync
        var mockAlunoRepo = new Moq.Mock<BibliotecaAPI.Repositories.IAlunoRepository>();
        mockAlunoRepo.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(new Aluno { Id = 1, Nome = "Teste" });
        
        var mockLivroRepo = new Moq.Mock<BibliotecaAPI.Repositories.ILivroRepository>();
        mockLivroRepo.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(new Livro { Id = 1, Titulo = "Livro", Quantidade = 5 });
        
        var mockEmprestimoRepo = new Moq.Mock<BibliotecaAPI.Repositories.IEmprestimoRepository>();
        mockEmprestimoRepo.Setup(r => r.PossuiEmprestimoAtivoAsync(1, 1)).ReturnsAsync(true);

        var service = new EmprestimoService(mockEmprestimoRepo.Object, mockLivroRepo.Object, mockAlunoRepo.Object);
        var dto = new BibliotecaAPI.DTOs.CriarEmprestimoDto { AlunoId = 1, LivroId = 1 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() => service.CriarAsync(dto));
        Assert.Equal("O aluno já possui um empréstimo ativo deste livro.", ex.Message);
    }
}
