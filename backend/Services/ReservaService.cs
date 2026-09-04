using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BibliotecaAPI.Services;

public interface IReservaService
{
    Task<IEnumerable<ReservaResponseDto>> ObterFilaReservaAsync(int livroId);
    Task<IEnumerable<ReservaResponseDto>> ObterReservasAlunoAsync(int alunoId);
    Task<ReservaResponseDto> CriarReservaAsync(CriarReservaDto dto);
}

public class ReservaService : IReservaService
{
    private readonly BibliotecaDbContext _context;
    private readonly IAuditoriaService? _auditoriaService;

    public ReservaService(BibliotecaDbContext context, IAuditoriaService? auditoriaService = null)
    {
        _context = context;
        _auditoriaService = auditoriaService;
    }

    public async Task<IEnumerable<ReservaResponseDto>> ObterFilaReservaAsync(int livroId)
    {
        var fila = await _context.Reservas
            .Include(r => r.Aluno)
            .Include(r => r.Livro)
            .Where(r => r.LivroId == livroId && r.Status == "Pendente")
            .OrderBy(r => r.DataReserva)
            .Select(r => new ReservaResponseDto
            {
                Id = r.Id,
                LivroId = r.LivroId,
                TituloLivro = r.Livro!.Titulo,
                AlunoId = r.AlunoId,
                NomeAluno = r.Aluno!.Nome,
                DataReserva = r.DataReserva,
                Status = r.Status
            })
            .ToListAsync();

        return fila;
    }

    public async Task<IEnumerable<ReservaResponseDto>> ObterReservasAlunoAsync(int alunoId)
    {
        return await _context.Reservas
            .Include(r => r.Aluno)
            .Include(r => r.Livro)
            .Where(r => r.AlunoId == alunoId)
            .OrderByDescending(r => r.DataReserva)
            .Select(r => new ReservaResponseDto
            {
                Id = r.Id,
                LivroId = r.LivroId,
                TituloLivro = r.Livro!.Titulo,
                AlunoId = r.AlunoId,
                NomeAluno = r.Aluno!.Nome,
                DataReserva = r.DataReserva,
                Status = r.Status
            })
            .ToListAsync();
    }

    public async Task<ReservaResponseDto> CriarReservaAsync(CriarReservaDto dto)
    {
        var livro = await _context.Livros.FirstOrDefaultAsync(l => l.Id == dto.LivroId)
            ?? throw new ArgumentException("Livro não encontrado");

        var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Id == dto.AlunoId)
            ?? throw new ArgumentException("Aluno não encontrado");

        var reservaAtiva = await _context.Reservas
            .AnyAsync(r => r.LivroId == dto.LivroId && r.AlunoId == dto.AlunoId && r.Status == "Pendente");
            
        if (reservaAtiva) throw new ArgumentException("Aluno já possui reserva pendente para este livro.");

        var reserva = new Reserva
        {
            LivroId = dto.LivroId,
            AlunoId = dto.AlunoId,
            DataReserva = DateTime.UtcNow,
            Status = "Pendente"
        };

        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                nomeUsuario: string.Empty,
                acao: "Criou Reserva",
                entidade: "Reserva",
                entidadeId: reserva.Id,
                detalhes: $"Reserva do livro '{livro.Titulo}' (ID {livro.Id}) solicitada pelo aluno '{aluno.Nome}' (ID {aluno.Id})"
            );
        }

        return new ReservaResponseDto
        {
            Id = reserva.Id,
            LivroId = reserva.LivroId,
            AlunoId = reserva.AlunoId,
            DataReserva = reserva.DataReserva,
            Status = reserva.Status
        };
    }
}
