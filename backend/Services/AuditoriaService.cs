using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BibliotecaAPI.Services;

public interface IAuditoriaService
{
    Task RegistrarAcaoAsync(string nomeUsuario, string acao, string entidade, int? entidadeId, string detalhes);
    Task<PagedResult<AuditoriaDto>> ObterLogsAsync(int page, int pageSize);
}

public class AuditoriaService : IAuditoriaService
{
    private readonly BibliotecaDbContext _context;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AuditoriaService(BibliotecaDbContext context, IHttpContextAccessor? httpContextAccessor = null)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task RegistrarAcaoAsync(string nomeUsuario, string acao, string entidade, int? entidadeId, string detalhes)
    {
        // Se o nome do usuário não foi passado explicitamente, resolve do contexto HTTP autenticado
        if (string.IsNullOrWhiteSpace(nomeUsuario))
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            nomeUsuario = user?.FindFirst(ClaimTypes.Email)?.Value 
                ?? user?.Identity?.Name 
                ?? user?.FindFirst("Nome")?.Value 
                ?? "Sistema";
        }

        var log = new LogAuditoria
        {
            DataHora = DateTime.UtcNow,
            NomeUsuario = nomeUsuario,
            Acao = acao,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Detalhes = detalhes
        };

        await _context.Auditoria.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<AuditoriaDto>> ObterLogsAsync(int page, int pageSize)
    {
        var query = _context.Auditoria.OrderByDescending(l => l.DataHora);

        var totalItens = await query.CountAsync();
        var totalPaginas = (int)Math.Ceiling(totalItens / (double)pageSize);

        var logs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AuditoriaDto
            {
                Id = l.Id,
                DataHora = l.DataHora,
                NomeUsuario = l.NomeUsuario,
                Acao = l.Acao,
                Entidade = l.Entidade,
                EntidadeId = l.EntidadeId,
                Detalhes = l.Detalhes
            })
            .ToListAsync();

        return new PagedResult<AuditoriaDto>
        {
            Itens = logs,
            PaginaAtual = page,
            TotalPaginas = totalPaginas,
            TotalItens = totalItens
        };
    }
}
