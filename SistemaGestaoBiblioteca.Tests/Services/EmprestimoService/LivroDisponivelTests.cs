using Xunit;
using BibliotecaAPI.Services;
using BibliotecaAPI.Models;

namespace SistemaGestaoBiblioteca.Tests.Services.EmprestimoServiceTests;

public class LivroDisponivelTests
{
    [Fact]
    public void DeveIndicarQueLivroEstaDisponivel()
    {
        // Arrange: Instancia o serviço e define um cenário com estoque positivo  
        var service = new EmprestimoService(null!, null!, null!);
        int quantidadeDisponivel = 3;
        // Act: Executa a validação de disponibilidade 
        var resultado = service.LivroDisponivel(quantidadeDisponivel);
        // Assert: Verifica se o sistema identifica corretamente a disponibilidade  
        Assert.True(resultado);
    }

    [Fact]
    public void DeveIndicarQueLivroNaoEstaDisponivel()
    {
        // Arrange: Define um cenário onde o estoque está zerado 
        var service = new EmprestimoService(null!, null!, null!);
        int quantidadeEsgotada = 0;
        // Act: Executa a validação 
        var resultado = service.LivroDisponivel(quantidadeEsgotada);
        // Assert: O retorno esperado deve ser falso 
        Assert.False(resultado);
    }
}
