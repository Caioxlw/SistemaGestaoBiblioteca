using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;

namespace BibliotecaAPI.Services;

public class AlunoService : IAlunoService
{
    private readonly IAlunoRepository _alunoRepository;

    public AlunoService(IAlunoRepository alunoRepository)
    {
        _alunoRepository = alunoRepository;
    }

    public async Task<AlunoResponseDto> CriarAsync(CriarAlunoDto dto)
    {
        if (await _alunoRepository.ExisteMatriculaAsync(dto.Matricula))
            throw new ConflictException("Já existe um aluno cadastrado com esta matrícula.");

        var aluno = new Aluno
        {
            Nome = dto.Nome,
            Matricula = dto.Matricula,
            Email = dto.Email
        };

        await _alunoRepository.AdicionarAsync(aluno);

        return new AlunoResponseDto
        {
            Id = aluno.Id,
            Nome = aluno.Nome,
            Matricula = aluno.Matricula,
            Email = aluno.Email
        };
    }

    public async Task<IEnumerable<AlunoResponseDto>> ObterTodosAsync()
    {
        var alunos = await _alunoRepository.ObterTodosAsync();
        return alunos.Select(a => new AlunoResponseDto
        {
            Id = a.Id,
            Nome = a.Nome,
            Matricula = a.Matricula,
            Email = a.Email
        });
    }

    public async Task<AlunoResponseDto> AtualizarAsync(int id, CriarAlunoDto dto)
    {
        var aluno = await _alunoRepository.ObterPorIdAsync(id) 
            ?? throw new NotFoundException($"Aluno com ID {id} não encontrado.");

        if (aluno.Matricula != dto.Matricula && await _alunoRepository.ExisteMatriculaAsync(dto.Matricula))
            throw new ConflictException("Já existe um aluno com esta matrícula.");

        aluno.Nome = dto.Nome;
        aluno.Matricula = dto.Matricula;
        aluno.Email = dto.Email;

        await _alunoRepository.AtualizarAsync(aluno);

        return new AlunoResponseDto
        {
            Id = aluno.Id,
            Nome = aluno.Nome,
            Matricula = aluno.Matricula,
            Email = aluno.Email
        };
    }

    public async Task ExcluirAsync(int id)
    {
        var aluno = await _alunoRepository.ObterPorIdAsync(id)
            ?? throw new NotFoundException($"Aluno com ID {id} não encontrado.");
            
        await _alunoRepository.ExcluirAsync(id);
    }
}