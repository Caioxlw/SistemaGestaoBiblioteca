using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BibliotecaAPI.Services;

public interface IDashboardService
{
    Task<DashboardDto> ObterDashboardAsync();
    Task<IEnumerable<RelatorioAtrasadoDto>> ObterEmprestimosAtrasadosAsync();
    Task<IEnumerable<RelatorioPopularDto>> ObterLivrosMaisPopularesAsync();
}

public class DashboardService : IDashboardService
{
    private readonly BibliotecaDbContext _context;
    private readonly ICacheService _cacheService;
    private const string CacheKeyPopulares = "livros:populares";

    public DashboardService(BibliotecaDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<DashboardDto> ObterDashboardAsync()
    {
        var totalLivros = await _context.Livros.SumAsync(l => l.Quantidade);
        var totalUsuarios = await _context.Alunos.CountAsync();
        
        var hoje = DateTime.UtcNow.Date;
        
        var emprestimosAtivos = await _context.Emprestimos
            .CountAsync(e => e.DataDevolucao == null);
            
        var livrosAtrasados = await _context.Emprestimos
            .CountAsync(e => e.DataDevolucao == null && e.DataPrevistaDevolucao.Date < hoje);

        var totalDevolvidos = await _context.Emprestimos
            .CountAsync(e => e.DataDevolucao != null);

        var categorias = await _context.Emprestimos
            .Include(e => e.Livro)
            .GroupBy(e => e.Livro!.Categoria)
            .Select(g => new CategoriaEmprestadaDto
            {
                Categoria = string.IsNullOrWhiteSpace(g.Key) ? "Outros" : g.Key,
                Total = g.Count()
            })
            .OrderByDescending(c => c.Total)
            .Take(5)
            .ToListAsync();

        // Cálculo de empréstimos por mês (últimos 6 meses)
        var seisMesesAtras = hoje.AddMonths(-5);
        var inicioPeriodo = new DateTime(seisMesesAtras.Year, seisMesesAtras.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        
        var emprestimosRecentes = await _context.Emprestimos
            .Where(e => e.DataEmprestimo >= inicioPeriodo)
            .Select(e => e.DataEmprestimo)
            .ToListAsync();

        var emprestimosPorMes = new List<EmprestimoMesDto>();
        for (int i = 5; i >= 0; i--)
        {
            var mesReferencia = hoje.AddMonths(-i);
            var totalMes = emprestimosRecentes.Count(d => d.Year == mesReferencia.Year && d.Month == mesReferencia.Month);
            emprestimosPorMes.Add(new EmprestimoMesDto
            {
                Mes = mesReferencia.ToString("MMM/yy"),
                Total = totalMes
            });
        }

        var statusEmprestimos = new StatusEmprestimosDto
        {
            NoPrazo = Math.Max(0, emprestimosAtivos - livrosAtrasados),
            Atrasados = livrosAtrasados,
            Devolvidos = totalDevolvidos
        };

        return new DashboardDto
        {
            TotalLivros = totalLivros,
            TotalUsuarios = totalUsuarios,
            EmprestimosAtivos = emprestimosAtivos,
            LivrosAtrasados = livrosAtrasados,
            CategoriasMaisEmprestadas = categorias,
            EmprestimosPorMes = emprestimosPorMes,
            StatusEmprestimos = statusEmprestimos
        };
    }

    public async Task<IEnumerable<RelatorioAtrasadoDto>> ObterEmprestimosAtrasadosAsync()
    {
        var hoje = DateTime.UtcNow.Date;
        
        var atrasados = await _context.Emprestimos
            .Include(e => e.Aluno)
            .Include(e => e.Livro)
            .Where(e => e.DataDevolucao == null && e.DataPrevistaDevolucao.Date < hoje)
            .ToListAsync();

        return atrasados.Select(e => {
            var diasAtraso = (hoje - e.DataPrevistaDevolucao.Date).Days;
            return new RelatorioAtrasadoDto
            {
                NomeAluno = e.Aluno?.Nome ?? "",
                EmailAluno = e.Aluno?.Email ?? "",
                TituloLivro = e.Livro?.Titulo ?? "",
                DataPrevistaDevolucao = e.DataPrevistaDevolucao,
                DiasAtraso = diasAtraso,
                Multa = diasAtraso * 2.0m // Multa de 2 reais por dia
            };
        }).OrderByDescending(a => a.DiasAtraso);
    }

    public async Task<IEnumerable<RelatorioPopularDto>> ObterLivrosMaisPopularesAsync()
    {
        // Padrão Cache-Aside com Redis:
        // 1. Consulta o Redis primeiro
        var cached = await _cacheService.GetAsync<List<RelatorioPopularDto>>(CacheKeyPopulares);
        if (cached != null && cached.Count > 0)
        {
            // Cache Hit -> retorna imediatamente
            return cached;
        }

        // Cache Miss -> busca no PostgreSQL via EF Core
        var populares = await _context.Emprestimos
            .Include(e => e.Livro)
            .Where(e => e.Livro != null)
            .GroupBy(e => e.Livro!.Titulo)
            .Select(g => new RelatorioPopularDto
            {
                Titulo = g.Key,
                TotalEmprestimos = g.Count()
            })
            .OrderByDescending(r => r.TotalEmprestimos)
            .Take(10)
            .ToListAsync();

        // Grava no Redis para próximas requisições (TTL 30 minutos)
        await _cacheService.SetAsync(CacheKeyPopulares, populares, expirationMinutes: 30);

        return populares;
    }
}
