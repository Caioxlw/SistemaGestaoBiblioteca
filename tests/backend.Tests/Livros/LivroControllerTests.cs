using Xunit;
using Moq;
using BibliotecaAPI.Controllers;
using BibliotecaAPI.Services;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace backend.Tests.Livros
{
    public class LivroControllerTests
    {
        private readonly Mock<ILivroService> _livroServiceMock;
        private readonly LivrosController _controller;

        public LivroControllerTests()
        {
            _livroServiceMock = new Mock<ILivroService>();
            _controller = new LivrosController(_livroServiceMock.Object);
        }

        [Fact]
        public async Task Criar_DeveRetornarStatus201_QuandoSucesso()
        {
            // Arrange
            var dto = new CriarLivroDto { Titulo = "Livro Teste", Isbn = "123", AnoPublicacao = 2020, Quantidade = 5, AutorId = 1 };
            var responseDto = new LivroResponseDto { Id = 1, Titulo = "Livro Teste", Isbn = "123", AnoPublicacao = 2020, Quantidade = 5, AutorId = 1, NomeAutor = "Autor Teste" };

            _livroServiceMock.Setup(s => s.CriarAsync(dto))
                .ReturnsAsync(responseDto);

            // Act
            var resultado = await _controller.Criar(dto);

            // Assert
            var objectResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            Assert.Equal(201, objectResult.StatusCode);
            Assert.Equal(responseDto, objectResult.Value);
        }
    }
}
