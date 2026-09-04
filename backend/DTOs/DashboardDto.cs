using System;
using System.Collections.Generic;

namespace BibliotecaAPI.DTOs;

public class DashboardDto
{
    public int TotalLivros { get; set; }
    public int TotalUsuarios { get; set; }
    public int EmprestimosAtivos { get; set; }
    public int LivrosAtrasados { get; set; }
    public IEnumerable<CategoriaEmprestadaDto> CategoriasMaisEmprestadas { get; set; } = new List<CategoriaEmprestadaDto>();
    public IEnumerable<EmprestimoMesDto> EmprestimosPorMes { get; set; } = new List<EmprestimoMesDto>();
    public StatusEmprestimosDto StatusEmprestimos { get; set; } = new();
}

public class EmprestimoMesDto
{
    public string Mes { get; set; } = string.Empty;
    public int Total { get; set; }
}

public class StatusEmprestimosDto
{
    public int NoPrazo { get; set; }
    public int Atrasados { get; set; }
    public int Devolvidos { get; set; }
}

public class CategoriaEmprestadaDto
{
    public string Categoria { get; set; } = string.Empty;
    public int Total { get; set; }
}

public class RelatorioAtrasadoDto
{
    public string NomeAluno { get; set; } = string.Empty;
    public string EmailAluno { get; set; } = string.Empty;
    public string TituloLivro { get; set; } = string.Empty;
    public DateTime DataPrevistaDevolucao { get; set; }
    public int DiasAtraso { get; set; }
    public decimal Multa { get; set; }
}

public class RelatorioPopularDto
{
    public string Titulo { get; set; } = string.Empty;
    public int TotalEmprestimos { get; set; }
}
