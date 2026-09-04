using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BibliotecaAPI.Services;

public interface INotificationService
{
    Task NotificarProximoDaFilaAsync(int livroId);
    Task<IEnumerable<NotificacaoResponseDto>> ObterPorAlunoAsync(int alunoId);
    Task<bool> MarcarComoLidaAsync(int id, int alunoId);
}

public class NotificationService : INotificationService
{
    private readonly BibliotecaDbContext _context;

    public NotificationService(BibliotecaDbContext context)
    {
        _context = context;
    }

    public async Task NotificarProximoDaFilaAsync(int livroId)
    {
        // Busca a próxima reserva pendente em ordem estritamente cronológica (FIFO)
        var proximaReserva = await _context.Reservas
            .Include(r => r.Aluno)
            .Include(r => r.Livro)
            .Where(r => r.LivroId == livroId && r.Status == "Pendente")
            .OrderBy(r => r.DataReserva)
            .FirstOrDefaultAsync();

        if (proximaReserva != null)
        {
            // Atualiza status da reserva para Atendida
            proximaReserva.Status = "Atendida";

            var tituloLivro = proximaReserva.Livro?.Titulo ?? "Livro reservado";
            var nomeAluno = proximaReserva.Aluno?.Nome ?? "Aluno";

            // Cria o registro da notificação imediata
            var notificacao = new Notificacao
            {
                AlunoId = proximaReserva.AlunoId,
                Mensagem = $"Olá {nomeAluno}, o livro '{tituloLivro}' que você reservou já está disponível na biblioteca para retirada!",
                DataNotificacao = DateTime.UtcNow,
                Tipo = "ReservaDisponivel",
                Lida = false
            };

            _context.Notificacoes.Add(notificacao);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<NotificacaoResponseDto>> ObterPorAlunoAsync(int alunoId)
    {
        return await _context.Notificacoes
            .Include(n => n.Aluno)
            .Where(n => n.AlunoId == alunoId)
            .OrderByDescending(n => n.DataNotificacao)
            .Select(n => new NotificacaoResponseDto
            {
                Id = n.Id,
                AlunoId = n.AlunoId,
                NomeAluno = n.Aluno != null ? n.Aluno.Nome : string.Empty,
                Mensagem = n.Mensagem,
                DataNotificacao = n.DataNotificacao,
                Tipo = n.Tipo,
                Lida = n.Lida
            })
            .ToListAsync();
    }

    public async Task<bool> MarcarComoLidaAsync(int id, int alunoId)
    {
        var notif = await _context.Notificacoes.FirstOrDefaultAsync(n => n.Id == id && n.AlunoId == alunoId);
        if (notif == null) return false;
        notif.Lida = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
