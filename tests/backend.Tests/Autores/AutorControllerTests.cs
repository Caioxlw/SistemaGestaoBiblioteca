using Xunit;
using Moq;
using BibliotecaAPI.Controllers;
using BibliotecaAPI.Services;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace backend.Tests.Autores
{
    public class AutorControllerTests
    {
        private readonly Mock<IAutorService> _autorServiceMock;
        private readonly AutoresController _controller;

        public AutorControllerTests()
        {
            _autorServiceMock = new Mock<IAutorService>();
            _controller = new AutoresController(_autorServiceMock.Object);
        }

        [Fact]
        public async Task Criar_DeveRetornarStatus201_QuandoSucesso()
        {
            // Arrange
            var dto = new CriarAutorDto { Nome = "J.K. Rowling", Nacionalidade = "Britânica", DataNascimento = new DateTime(1965, 7, 31) };
            var responseDto = new AutorResponseDto { Id = 1, Nome = "J.K. Rowling", Nacionalidade = "Britânica", DataNascimento = new DateTime(1965, 7, 31) };

            _autorServiceMock.Setup(s => s.CriarAsync(dto))
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
