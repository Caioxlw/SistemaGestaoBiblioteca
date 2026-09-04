using Xunit;
using Moq;
using BibliotecaAPI.Services;
using BibliotecaAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaGestaoBiblioteca.Tests.Services;

public class LivrosPopularesCacheTests
{
    [Fact]
    public async Task ObterLivrosMaisPopularesAsync_DeveRetornarDoCacheQuandoExistir()
    {
        // Arrange (Cache Hit)
        var mockCache = new Mock<ICacheService>();
        var listaEmCache = new List<RelatorioPopularDto>
        {
            new() { Titulo = "Dom Casmurro", TotalEmprestimos = 15 },
            new() { Titulo = "1984", TotalEmprestimos = 12 }
        };

        mockCache.Setup(c => c.GetAsync<List<RelatorioPopularDto>>("livros:populares"))
            .ReturnsAsync(listaEmCache);

        var service = new DashboardService(null!, mockCache.Object);

        // Act
        var resultado = await service.ObterLivrosMaisPopularesAsync();

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, ((List<RelatorioPopularDto>)resultado).Count);
        mockCache.Verify(c => c.GetAsync<List<RelatorioPopularDto>>("livros:populares"), Times.Once);
        // Garante que SetAsync NÃO é chamado quando há cache hit
        mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<List<RelatorioPopularDto>>(), It.IsAny<int>()), Times.Never);
    }
}
