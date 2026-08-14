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
}