using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Bibliotecario")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("api/dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var dashboard = await _dashboardService.ObterDashboardAsync();
        return Ok(dashboard);
    }

    [HttpGet("api/relatorios/atrasados")]
    public async Task<ActionResult<IEnumerable<RelatorioAtrasadoDto>>> GetAtrasados()
    {
        var atrasados = await _dashboardService.ObterEmprestimosAtrasadosAsync();
        return Ok(atrasados);
    }

    [HttpGet("api/relatorios/populares")]
    public async Task<ActionResult<IEnumerable<RelatorioPopularDto>>> GetPopulares()
    {
        var populares = await _dashboardService.ObterLivrosMaisPopularesAsync();
        return Ok(populares);
    }
}
