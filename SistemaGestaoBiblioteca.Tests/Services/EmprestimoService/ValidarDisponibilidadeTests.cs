using Xunit;
using BibliotecaAPI.Services;
using BibliotecaAPI.Models;
using BibliotecaAPI.Exceptions;

namespace SistemaGestaoBiblioteca.Tests.Services.EmprestimoServiceTests;

public class ValidarDisponibilidadeTests
{
    [Fact]
    public void DeveLancarExcecaoQuandoLivroIndisponivel()
    {
        // Arrange: Configura cenário crítico de estoque zerado 
        var service = new EmprestimoService(null!, null!, null!);
        int quantidadeIndisponivel = 0;
        // Act & Assert: Verifica se a exceção RegraNegocioException é disparada 
        Assert.Throws<RegraNegocioException>(
            () => service.ValidarDisponibilidade(quantidadeIndisponivel)
        );
    }
}
