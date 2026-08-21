using Xunit;
using Moq;
using BibliotecaAPI.Services;
using BibliotecaAPI.Repositories;
using BibliotecaAPI.Models;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.DTOs;
using System.Threading.Tasks;

namespace backend.Tests.Livros
{
    public class LivroServiceTests
    {
        private readonly Mock<ILivroRepository> _livroRepositoryMock;
        private readonly Mock<IAutorRepository> _autorRepositoryMock;
        private readonly LivroService _livroService;

        public LivroServiceTests()
        {
            _livroRepositoryMock = new Mock<ILivroRepository>();
            _autorRepositoryMock = new Mock<IAutorRepository>();
            _livroService = new LivroService(_livroRepositoryMock.Object, _autorRepositoryMock.Object);
        }

        [Fact]
        public async Task CriarAsync_DeveLancarExcecao_QuandoAutorNaoExiste()
        {
            // Arrange
            var dto = new CriarLivroDto { Titulo = "Livro Teste", Isbn = "123", AnoPublicacao = 2020, Quantidade = 5, AutorId = 99 };
            
            _autorRepositoryMock.Setup(r => r.ObterPorIdAsync(dto.AutorId))
                .ReturnsAsync((Autor)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _livroService.CriarAsync(dto));
            Assert.Equal($"Autor com ID {dto.AutorId} não existe.", ex.Message);
        }

        [Fact]
        public async Task CriarAsync_DeveRetornarSucesso_QuandoAutorExiste()
        {
            // Arrange
            var dto = new CriarLivroDto { Titulo = "Livro Teste", Isbn = "123", AnoPublicacao = 2020, Quantidade = 5, AutorId = 1 };
            var autor = new Autor { Id = 1, Nome = "Autor Teste" };
            
            _autorRepositoryMock.Setup(r => r.ObterPorIdAsync(dto.AutorId))
                .ReturnsAsync(autor);

            _livroRepositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Livro>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _livroService.CriarAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(dto.Titulo, resultado.Titulo);
            Assert.Equal(autor.Nome, resultado.NomeAutor);
            
            _livroRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Livro>()), Times.Once);
        }
    }
}
