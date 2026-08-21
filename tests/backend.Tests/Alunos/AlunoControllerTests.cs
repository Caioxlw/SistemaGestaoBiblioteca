using Xunit;
using Moq;
using BibliotecaAPI.Controllers;
using BibliotecaAPI.Services;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace backend.Tests.Alunos
{
    public class AlunoControllerTests
    {
        private readonly Mock<IAlunoService> _alunoServiceMock;
        private readonly AlunosController _controller;

        public AlunoControllerTests()
        {
            _alunoServiceMock = new Mock<IAlunoService>();
            _controller = new AlunosController(_alunoServiceMock.Object);
        }

        [Fact]
        public async Task Criar_DeveRetornarStatus201_QuandoSucesso()
        {
            // Arrange
            var dto = new CriarAlunoDto { Nome = "Maria", Matricula = "123", Email = "maria@escola.com" };
            var responseDto = new AlunoResponseDto { Id = 1, Nome = "Maria", Matricula = "123", Email = "maria@escola.com" };

            _alunoServiceMock.Setup(s => s.CriarAsync(dto))
                .ReturnsAsync(responseDto);

            // Act
            var resultado = await _controller.Criar(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(resultado.Result);
            Assert.Equal(201, objectResult.StatusCode);
            Assert.Equal(responseDto, objectResult.Value);
        }
    }
}
