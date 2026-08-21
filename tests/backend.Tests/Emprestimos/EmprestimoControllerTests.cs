using Xunit;
using Moq;
using BibliotecaAPI.Controllers;
using BibliotecaAPI.Services;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace backend.Tests.Emprestimos
{
    public class EmprestimoControllerTests
    {
        private readonly Mock<IEmprestimoService> _emprestimoServiceMock;
        private readonly EmprestimosController _controller;

        public EmprestimoControllerTests()
        {
            _emprestimoServiceMock = new Mock<IEmprestimoService>();
            _controller = new EmprestimosController(_emprestimoServiceMock.Object);
        }

        [Fact]
        public async Task Criar_DeveRetornarStatus201_QuandoSucesso()
        {
            // Arrange
            var dto = new CriarEmprestimoDto { AlunoId = 1, LivroId = 1 };
            var responseDto = new EmprestimoResponseDto { Id = 1, AlunoId = 1, LivroId = 1 };

            _emprestimoServiceMock.Setup(s => s.CriarAsync(dto))
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
