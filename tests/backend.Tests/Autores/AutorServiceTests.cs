using Xunit;
using Moq;
using BibliotecaAPI.Services;
using BibliotecaAPI.Repositories;
using BibliotecaAPI.Models;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.DTOs;
using System.Threading.Tasks;
using System;

namespace backend.Tests.Autores
{
    public class AutorServiceTests
    {
        private readonly Mock<IAutorRepository> _autorRepositoryMock;
        private readonly AutorService _autorService;

        public AutorServiceTests()
        {
            _autorRepositoryMock = new Mock<IAutorRepository>();
            _autorService = new AutorService(_autorRepositoryMock.Object);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoNaoEncontrado()
        {
            // Arrange
            _autorRepositoryMock.Setup(r => r.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync((BibliotecaAPI.Models.Autor)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _autorService.ObterPorIdAsync(999));
            Assert.Contains("não foi encontrado", ex.Message);
        }

        [Fact]
        public async Task CriarAsync_DeveRetornarSucesso_QuandoDadosValidos()
        {
            // Arrange
            var dto = new CriarAutorDto { Nome = "J.K. Rowling", Nacionalidade = "Britânica", DataNascimento = new DateTime(1965, 7, 31) };

            // Act
            var resultado = await _autorService.CriarAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(dto.Nome, resultado.Nome);
            
            _autorRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<BibliotecaAPI.Models.Autor>()), Times.Once);
        }
    }
}
