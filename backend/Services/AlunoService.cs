using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BibliotecaAPI.Services;

public class AlunoService : IAlunoService
{
    private readonly IAlunoRepository _alunoRepository;
    private readonly BibliotecaDbContext _context;
    private readonly IAuditoriaService? _auditoriaService;

    public AlunoService(
        IAlunoRepository alunoRepository,
        BibliotecaDbContext context,
        IAuditoriaService? auditoriaService = null)
    {
        _alunoRepository = alunoRepository;
        _context = context;
        _auditoriaService = auditoriaService;
    }

    public async Task<AlunoResponseDto> CriarAsync(CriarAlunoDto dto)
    {
        var nome = dto.Nome?.Trim() ?? string.Empty;
        var matricula = dto.Matricula?.Trim() ?? string.Empty;
        var email = dto.Email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException("O nome do aluno é obrigatório.");

        if (string.IsNullOrWhiteSpace(matricula))
            throw new RegraNegocioException("A matrícula do aluno é obrigatória.");

        if (string.IsNullOrWhiteSpace(email))
            throw new RegraNegocioException("O e-mail do aluno é obrigatório.");

        if (await _alunoRepository.ExisteMatriculaAsync(matricula))
            throw new ConflictException("Já existe um aluno cadastrado com esta matrícula.");

        if (await _alunoRepository.ExisteEmailAsync(email) ||
            await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
        {
            throw new ConflictException("Já existe um usuário ou aluno com este e-mail cadastrado.");
        }

        var aluno = new Aluno
        {
            Nome = nome,
            Matricula = matricula,
            Email = email
        };

        await _alunoRepository.AdicionarAsync(aluno);

        // Cria automaticamente o usuário de acesso para o aluno
        var usuario = new Usuario
        {
            Nome = aluno.Nome,
            Email = aluno.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("Aluno@123"),
            Perfil = PerfilUsuario.Aluno,
            AlunoId = aluno.Id
        };
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                nomeUsuario: string.Empty,
                acao: "Cadastrou Aluno",
                entidade: "Aluno",
                entidadeId: aluno.Id,
                detalhes: $"Aluno '{aluno.Nome}' (Matrícula: {aluno.Matricula}, Email: {aluno.Email}) cadastrado com conta de acesso vinculada."
            );
        }

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

        var nome = dto.Nome?.Trim() ?? string.Empty;
        var matricula = dto.Matricula?.Trim() ?? string.Empty;
        var email = dto.Email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException("O nome do aluno é obrigatório.");

        if (string.IsNullOrWhiteSpace(matricula))
            throw new RegraNegocioException("A matrícula do aluno é obrigatória.");

        if (string.IsNullOrWhiteSpace(email))
            throw new RegraNegocioException("O e-mail do aluno é obrigatório.");

        if (aluno.Matricula != matricula && await _alunoRepository.ExisteMatriculaAsync(matricula, id))
            throw new ConflictException("Já existe outro aluno com esta matrícula.");

        if (!aluno.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _alunoRepository.ExisteEmailAsync(email, id) ||
                await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == email.ToLower() && u.AlunoId != id))
            {
                throw new ConflictException("Já existe outro usuário ou aluno com este e-mail cadastrado.");
            }
        }

        aluno.Nome = nome;
        aluno.Matricula = matricula;
        aluno.Email = email;

        await _alunoRepository.AtualizarAsync(aluno);

        // Atualiza ou cria a conta de usuário vinculada
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.AlunoId == id);
        if (usuario != null)
        {
            usuario.Nome = aluno.Nome;
            usuario.Email = aluno.Email;
            _context.Usuarios.Update(usuario);
        }
        else
        {
            usuario = new Usuario
            {
                Nome = aluno.Nome,
                Email = aluno.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Aluno@123"),
                Perfil = PerfilUsuario.Aluno,
                AlunoId = aluno.Id
            };
            await _context.Usuarios.AddAsync(usuario);
        }
        await _context.SaveChangesAsync();

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                nomeUsuario: string.Empty,
                acao: "Atualizou Aluno",
                entidade: "Aluno",
                entidadeId: aluno.Id,
                detalhes: $"Dados do aluno '{aluno.Nome}' atualizados."
            );
        }

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

        var temEmprestimosPendentes = await _context.Emprestimos.AnyAsync(e => e.AlunoId == id && e.Status != StatusEmprestimo.Devolvido);
        if (temEmprestimosPendentes)
            throw new ConflictException("Não é possível excluir o aluno pois ele possui empréstimos pendentes.");

        var temReservasPendentes = await _context.Reservas.AnyAsync(r => r.AlunoId == id && r.Status == "Pendente");
        if (temReservasPendentes)
            throw new ConflictException("Não é possível excluir o aluno pois ele possui reservas pendentes.");

        // Remove o usuário associado
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.AlunoId == id);
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
        }

        await _alunoRepository.ExcluirAsync(id);

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                nomeUsuario: string.Empty,
                acao: "Excluiu Aluno",
                entidade: "Aluno",
                entidadeId: id,
                detalhes: $"Aluno '{aluno.Nome}' (ID: {id}) foi excluído."
            );
        }
    }
}