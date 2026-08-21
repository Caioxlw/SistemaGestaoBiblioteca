using Xunit;
using Moq;
using BibliotecaAPI.Services;
using BibliotecaAPI.Repositories;
using BibliotecaAPI.Models;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.DTOs;
using System;
using System.Threading.Tasks;

namespace backend.Tests.Emprestimos
{
    public class EmprestimoServiceTests
    {
        private readonly Mock<IEmprestimoRepository> _emprestimoRepositoryMock = new();
        private readonly Mock<ILivroRepository> _livroRepositoryMock = new();
        private readonly Mock<IAlunoRepository> _alunoRepositoryMock = new();
        private readonly EmprestimoService _emprestimoService;

        public EmprestimoServiceTests()
        {
            _emprestimoService = new EmprestimoService(
                _emprestimoRepositoryMock.Object,
                _livroRepositoryMock.Object,
                _alunoRepositoryMock.Object
            );
        }

        private CriarEmprestimoDto SetupCriacaoPadrao(int estoque = 5)
        {
            _alunoRepositoryMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(new Aluno { Id = 1, Nome = "João" });
            _livroRepositoryMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(new Livro { Id = 1, Quantidade = estoque });
            return new CriarEmprestimoDto { AlunoId = 1, LivroId = 1, DataPrevistaDevolucao = DateTime.Now.AddDays(7) };
        }

        [Fact]
        public async Task CriarAsync_DeveLancarExcecao_QuandoEstoqueInsuficiente()
        {
            var dto = SetupCriacaoPadrao(estoque: 0);

            var ex = await Assert.ThrowsAsync<ConflictException>(() => _emprestimoService.CriarAsync(dto));
            Assert.Equal("O livro não possui exemplares disponíveis.", ex.Message);
        }

        [Fact]
        public async Task CriarAsync_DeveLancarExcecao_QuandoEmprestimoDuplicadoAtivo()
        {
            var dto = SetupCriacaoPadrao();
            _emprestimoRepositoryMock.Setup(r => r.PossuiEmprestimoAtivoAsync(1, 1)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ConflictException>(() => _emprestimoService.CriarAsync(dto));
            Assert.Equal("O aluno já possui um empréstimo ativo deste mesmo livro.", ex.Message);
        }

        [Fact]
        public async Task DevolverAsync_DeveLancarExcecao_QuandoJaDevolvido()
        {
            var emprestimo = new Emprestimo { Id = 1, Status = StatusEmprestimo.Devolvido };
            _emprestimoRepositoryMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(emprestimo);

            var ex = await Assert.ThrowsAsync<ConflictException>(() => _emprestimoService.DevolverAsync(1));
            Assert.Equal("Este empréstimo já foi devolvido.", ex.Message);
        }

        [Fact]
        public async Task CriarAsync_DeveRetornarSucesso_QuandoDadosValidos()
        {
            var dto = SetupCriacaoPadrao(estoque: 5);
            _emprestimoRepositoryMock.Setup(r => r.PossuiEmprestimoAtivoAsync(1, 1)).ReturnsAsync(false);

            var resultado = await _emprestimoService.CriarAsync(dto);

            Assert.NotNull(resultado);
            _emprestimoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Emprestimo>()), Times.Once);
            _livroRepositoryMock.Verify(r => r.AtualizarAsync(It.Is<Livro>(l => l.Quantidade == 4)), Times.Once);
        }
    }
}
