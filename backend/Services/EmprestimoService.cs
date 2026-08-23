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
        // Adaptado do serviço antigo: agora recebe DTO e retorna o DTO de resposta da API.
        var aluno = await _alunoRepository.ObterPorIdAsync(dto.AlunoId)
            ?? throw new NotFoundException($"Aluno com ID {dto.AlunoId} não encontrado.");

        var livro = await _livroRepository.ObterPorIdAsync(dto.LivroId)
            ?? throw new NotFoundException($"Livro com ID {dto.LivroId} não encontrado.");

        // CORRIGIDO: bloqueia empréstimo quando estoque é zero ou negativo.
        if (livro.Quantidade <= 0)
            throw new ConflictException("Livro não possui exemplares disponíveis.");

        if (await _emprestimoRepository.PossuiEmprestimoAtivoAsync(dto.AlunoId, dto.LivroId))
            throw new ConflictException("O aluno já possui um empréstimo ativo deste livro.");

        var emprestimo = new Emprestimo
        {
            AlunoId = dto.AlunoId,
            LivroId = dto.LivroId,
            DataEmprestimo = DateTime.UtcNow,
            // Lógica antiga mantida: ignora a data enviada e fixa o prazo em 14 dias.
            DataPrevistaDevolucao = DateTime.UtcNow.AddDays(14),
            Status = StatusEmprestimo.Ativo
        };

        // Lógica normal do empréstimo: reduz a quantidade disponível.
        livro.Quantidade--;
        await _livroRepository.AtualizarAsync(livro);
        await _emprestimoRepository.AdicionarAsync(emprestimo);

        emprestimo.Aluno = aluno;
        emprestimo.Livro = livro;
        return Mapear(emprestimo);
    }

    public async Task<EmprestimoResponseDto> DevolverAsync(int id)
    {
        // Adaptado do serviço antigo para retornar EmprestimoResponseDto.
        var emprestimo = await _emprestimoRepository.ObterPorIdAsync(id)
            ?? throw new NotFoundException($"Empréstimo com ID {id} não encontrado.");

        if (emprestimo.Status == StatusEmprestimo.Devolvido)
            throw new ConflictException("Este empréstimo já foi devolvido.");

        var livro = await _livroRepository.ObterPorIdAsync(emprestimo.LivroId)
            ?? throw new NotFoundException("Livro associado ao empréstimo não encontrado.");

        emprestimo.DataDevolucao = DateTime.UtcNow;
        emprestimo.Status = StatusEmprestimo.Devolvido;
        // CORRIGIDO: a devolução agora incrementa o estoque corretamente.
        livro.Quantidade++;

        await _livroRepository.AtualizarAsync(livro);
        await _emprestimoRepository.AtualizarAsync(emprestimo);

        emprestimo.Livro = livro;
        return Mapear(emprestimo);
    }

    public async Task<IEnumerable<EmprestimoResponseDto>> ObterTodosAsync() =>
        // Método renomeado para acompanhar a interface atual; substitui ListarAsync.
        (await _emprestimoRepository.ObterTodosAsync()).Select(Mapear);

    public async Task<IEnumerable<EmprestimoResponseDto>> ObterAbertosAsync() =>
        (await _emprestimoRepository.ObterAbertosAsync()).Select(Mapear);

    public async Task<IEnumerable<EmprestimoResponseDto>> ObterPorAlunoAsync(int alunoId) =>
        (await _emprestimoRepository.ObterPorAlunoAsync(alunoId)).Select(Mapear);

    public bool LivroDisponivel(int quantidade)
    {
        // Lógica antiga mantida: quantidade zero é considerada indisponível aqui,
        // embora CriarAsync permita o empréstimo nesse caso.
        return quantidade > 0;
    }

    public decimal CalcularMulta(int diasAtraso)
    {
        // Lógica atualizada para R$ 2,00 conforme exercício.
        const decimal valorPorDia = 2.00m;

        if (diasAtraso <= 0)
            return 0;

        return diasAtraso * valorPorDia;
    }

    public void ValidarDisponibilidade(int quantidade)
    {
        if (quantidade <= 0)
            throw new RegraNegocioException("Livro indisponível para empréstimo.");
    }

    private static EmprestimoResponseDto Mapear(Emprestimo emprestimo) => new()
    {
        Id = emprestimo.Id,
        AlunoId = emprestimo.AlunoId,
        NomeAluno = emprestimo.Aluno?.Nome ?? string.Empty,
        LivroId = emprestimo.LivroId,
        TituloLivro = emprestimo.Livro?.Titulo ?? string.Empty,
        DataEmprestimo = emprestimo.DataEmprestimo,
        DataPrevistaDevolucao = emprestimo.DataPrevistaDevolucao,
        DataDevolucao = emprestimo.DataDevolucao,
        Status = emprestimo.Status.ToString()
    };
}