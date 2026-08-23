using Xunit;
using BibliotecaAPI.Services;
using BibliotecaAPI.Models;

namespace SistemaGestaoBiblioteca.Tests.Services.EmprestimoServiceTests;

public class CalcularMultaTests
{
    [Theory]
    [InlineData(1, 2)] // 1 dia de atraso -> R$ 2,00 
    [InlineData(5, 10)] // 5 dias de atraso -> R$ 10,00 
    [InlineData(10, 20)]// 10 dias de atraso -> R$ 20,00 
    public void DeveCalcularMulta(int dias, decimal valorEsperado)
    {
        // Arrange: Setup do serviço de empréstimo 
        var service = new EmprestimoService(null!, null!, null!);
        // Act: Processamento do cálculo baseado nos inputs da teoria 
        var resultado = service.CalcularMulta(dias);
        // Assert: Validação do valor calculado contra a expectativa do negócio 
        Assert.Equal(valorEsperado, resultado);
    }
}
