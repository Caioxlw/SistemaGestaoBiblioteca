using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _auditoriaService;

    public AuditoriaController(IAuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AuditoriaDto>>> ObterLogs(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        var logs = await _auditoriaService.ObterLogsAsync(page, pageSize);
        return Ok(logs);
    }
}
