using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;

namespace BibliotecaAPI.Services;

public class EmprestimoService : IEmprestimoService
{
    private readonly IEmprestimoRepository _emprestimoRepository;
    private readonly ILivroRepository _livroRepository;
    private readonly IAlunoRepository _alunoRepository;

    public EmprestimoService(
        IEmprestimoRepository emprestimoRepository,
        ILivroRepository livroRepository,
        IAlunoRepository alunoRepository)
    {
        _emprestimoRepository = emprestimoRepository;
        _livroRepository = livroRepository;
        _alunoRepository = alunoRepository;
    }

    public async Task<EmprestimoResponseDto> CriarAsync(CriarEmprestimoDto dto)
    {
        var aluno = await _alunoRepository.ObterPorIdAsync(dto.AlunoId)
            ?? throw new NotFoundException($"Aluno com ID {dto.AlunoId} não foi encontrado.");

        var livro = await _livroRepository.ObterPorIdAsync(dto.LivroId)
            ?? throw new NotFoundException($"Livro com ID {dto.LivroId} não foi encontrado.");

        // Regra de Negócio 1: Estoque insuficiente
        if (livro.Quantidade <= 0)
            throw new ConflictException("O livro não possui exemplares disponíveis.");

        // Regra de Negócio 2: Empréstimo duplicado ativo
        if (await _emprestimoRepository.PossuiEmprestimoAtivoAsync(dto.AlunoId, dto.LivroId))
            throw new ConflictException("O aluno já possui um empréstimo ativo deste mesmo livro.");

        var emprestimo = new Emprestimo
        {
            AlunoId = dto.AlunoId,
            LivroId = dto.LivroId,
            DataEmprestimo = DateTime.Now,
            DataPrevistaDevolucao = dto.DataPrevistaDevolucao,
            Status = StatusEmprestimo.Ativo
        };

        // Decrementa o estoque
        livro.Quantidade -= 1;

        await _emprestimoRepository.AdicionarAsync(emprestimo);
        await _livroRepository.AtualizarAsync(livro);

        return MapToResponseDto(emprestimo, aluno.Nome, livro.Titulo);
    }

    public async Task<EmprestimoResponseDto> DevolverAsync(int id)
    {
        var emprestimo = await _emprestimoRepository.ObterPorIdAsync(id)
            ?? throw new NotFoundException($"Empréstimo com ID {id} não foi encontrado.");

        // Regra de Negócio 3: Devolução duplicada
        if (emprestimo.Status == StatusEmprestimo.Devolvido)
            throw new ConflictException("Este empréstimo já foi devolvido.");

        emprestimo.DataDevolucao = DateTime.Now;
        emprestimo.Status = StatusEmprestimo.Devolvido;

        // Incrementa o estoque
        if (emprestimo.Livro != null)
        {
            emprestimo.Livro.Quantidade += 1;
            await _livroRepository.AtualizarAsync(emprestimo.Livro);
        }

        await _emprestimoRepository.AtualizarAsync(emprestimo);

        return MapToResponseDto(
            emprestimo, 
            emprestimo.Aluno?.Nome ?? string.Empty, 
            emprestimo.Livro?.Titulo ?? string.Empty);
    }

    private static EmprestimoResponseDto MapToResponseDto(Emprestimo e, string nomeAluno, string tituloLivro)
    {
        return new EmprestimoResponseDto
        {
            Id = e.Id,
            AlunoId = e.AlunoId,
            NomeAluno = nomeAluno,
            LivroId = e.LivroId,
            TituloLivro = tituloLivro,
            DataEmprestimo = e.DataEmprestimo,
            DataPrevistaDevolucao = e.DataPrevistaDevolucao,
            DataDevolucao = e.DataDevolucao,
            Status = e.Status.ToString()
        };
    }
}