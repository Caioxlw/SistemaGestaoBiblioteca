using Xunit;
using Moq;
using BibliotecaAPI.Services;
using BibliotecaAPI.Repositories;
using BibliotecaAPI.Models;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.DTOs;
using System.Threading.Tasks;

namespace backend.Tests.Alunos
{
    public class AlunoServiceTests
    {
        private readonly Mock<IAlunoRepository> _alunoRepositoryMock;
        private readonly AlunoService _alunoService;

        public AlunoServiceTests()
        {
            _alunoRepositoryMock = new Mock<IAlunoRepository>();
            _alunoService = new AlunoService(_alunoRepositoryMock.Object);
        }

        [Fact]
        public async Task CriarAsync_DeveLancarExcecao_QuandoMatriculaJaExiste()
        {
            // Arrange
            var dto = new CriarAlunoDto { Matricula = "12345", Nome = "Maria", Email = "maria@escola.com" };
            
            _alunoRepositoryMock.Setup(r => r.ExisteMatriculaAsync(dto.Matricula))
                .ReturnsAsync(true); // Simula que já existe

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ConflictException>(() => _alunoService.CriarAsync(dto));
            Assert.Equal("Já existe um aluno cadastrado com esta matrícula.", ex.Message);
        }

        [Fact]
        public async Task CriarAsync_DeveRetornarSucesso_QuandoMatriculaForUnica()
        {
            // Arrange
            var dto = new CriarAlunoDto { Matricula = "12345", Nome = "Maria", Email = "maria@escola.com" };
            
            _alunoRepositoryMock.Setup(r => r.ExisteMatriculaAsync(dto.Matricula))
                .ReturnsAsync(false); // Simula que não existe

            // Act
            var resultado = await _alunoService.CriarAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(dto.Matricula, resultado.Matricula);
            Assert.Equal(dto.Nome, resultado.Nome);
            
            _alunoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<BibliotecaAPI.Models.Aluno>()), Times.Once);
        }
    }
}
