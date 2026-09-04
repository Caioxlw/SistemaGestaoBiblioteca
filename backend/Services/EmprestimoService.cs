using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;

namespace BibliotecaAPI.Services;

public class EmprestimoService : IEmprestimoService
{
    private const int PrazoPadraoEmprestimoDias = 14;
    private const decimal MultaPorDiaAtraso = 2.00m;

    private readonly IEmprestimoRepository _emprestimoRepository;
    private readonly ILivroRepository _livroRepository;
    private readonly IAlunoRepository _alunoRepository;
    private readonly INotificationService? _notificationService;
    private readonly IAuditoriaService? _auditoriaService;
    private readonly ICacheService? _cacheService;

    public EmprestimoService(
        IEmprestimoRepository emprestimoRepository,
        ILivroRepository livroRepository,
        IAlunoRepository alunoRepository,
        INotificationService? notificationService = null,
        IAuditoriaService? auditoriaService = null,
        ICacheService? cacheService = null)
    {
        _emprestimoRepository = emprestimoRepository;
        _livroRepository = livroRepository;
        _alunoRepository = alunoRepository;
        _notificationService = notificationService;
        _auditoriaService = auditoriaService;
        _cacheService = cacheService;
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

        var dataEmprestimo = DateTime.UtcNow;

        var emprestimo = new Emprestimo
        {
            AlunoId = dto.AlunoId,
            LivroId = dto.LivroId,
            DataEmprestimo = dataEmprestimo,
            DataPrevistaDevolucao = dataEmprestimo.AddDays(PrazoPadraoEmprestimoDias),
            Status = StatusEmprestimo.Ativo
        };

        // Lógica normal do empréstimo: reduz a quantidade disponível.
        livro.Quantidade--;
        await _livroRepository.AtualizarAsync(livro);
        await _emprestimoRepository.AdicionarAsync(emprestimo);

        if (_cacheService != null)
        {
            await _cacheService.RemoveAsync("livros:populares");
        }

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                nomeUsuario: string.Empty,
                acao: "Criou Empréstimo",
                entidade: "Emprestimo",
                entidadeId: emprestimo.Id,
                detalhes: $"Empréstimo do livro '{livro.Titulo}' (ID {livro.Id}) para o aluno '{aluno.Nome}' (ID {aluno.Id})"
            );
        }

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

        // DISPARO IMEDIATO DA NOTIFICAÇÃO: Se houver reserva pendente, avisa o próximo da fila
        if (_notificationService != null)
        {
            await _notificationService.NotificarProximoDaFilaAsync(livro.Id);
        }

        if (_cacheService != null)
        {
            await _cacheService.RemoveAsync("livros:populares");
        }

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                nomeUsuario: string.Empty,
                acao: "Registrou Devolução",
                entidade: "Emprestimo",
                entidadeId: emprestimo.Id,
                detalhes: $"Devolução concluída do livro '{livro.Titulo}' (ID {livro.Id})"
            );
        }

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
        if (diasAtraso <= 0)
            return 0;

        return diasAtraso * MultaPorDiaAtraso;
    }

    public void ValidarDisponibilidade(int quantidade)
    {
        if (quantidade <= 0)
            throw new RegraNegocioException("Livro indisponível para empréstimo.");
    }

    private EmprestimoResponseDto Mapear(Emprestimo emprestimo)
    {
        var referencia = emprestimo.DataDevolucao ?? DateTime.UtcNow;
        var diasAtraso = CalcularDiasAtraso(emprestimo.DataPrevistaDevolucao, referencia);

        return new EmprestimoResponseDto
        {
            Id = emprestimo.Id,
            AlunoId = emprestimo.AlunoId,
            NomeAluno = emprestimo.Aluno?.Nome ?? string.Empty,
            LivroId = emprestimo.LivroId,
            TituloLivro = emprestimo.Livro?.Titulo ?? string.Empty,
            DataEmprestimo = emprestimo.DataEmprestimo,
            DataPrevistaDevolucao = emprestimo.DataPrevistaDevolucao,
            DataDevolucao = emprestimo.DataDevolucao,
            DiasAtraso = diasAtraso,
            Multa = CalcularMulta(diasAtraso),
            Status = ObterStatusAtual(emprestimo, referencia).ToString()
        };
    }

    private static int CalcularDiasAtraso(DateTime dataPrevistaDevolucao, DateTime referencia)
    {
        if (referencia.Date <= dataPrevistaDevolucao.Date)
            return 0;

        return (referencia.Date - dataPrevistaDevolucao.Date).Days;
    }

    private static StatusEmprestimo ObterStatusAtual(Emprestimo emprestimo, DateTime referencia)
    {
        if (emprestimo.Status == StatusEmprestimo.Devolvido)
            return StatusEmprestimo.Devolvido;

        return referencia.Date > emprestimo.DataPrevistaDevolucao.Date
            ? StatusEmprestimo.Atrasado
            : emprestimo.Status;
    }
}