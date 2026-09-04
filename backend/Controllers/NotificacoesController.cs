using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificacoesController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificacoesController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("aluno/{alunoId:int}")]
    public async Task<ActionResult<IEnumerable<NotificacaoResponseDto>>> ObterPorAluno(int alunoId)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var alunoIdClaim = User.FindFirst("AlunoId")?.Value;

        if (role == "Aluno" && alunoIdClaim != alunoId.ToString())
        {
            return Forbid();
        }

        var notificacoes = await _notificationService.ObterPorAlunoAsync(alunoId);
        return Ok(notificacoes);
    }

    [HttpPut("{id:int}/lida")]
    public async Task<IActionResult> MarcarComoLida(int id, [FromQuery] int alunoId)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var alunoIdClaim = User.FindFirst("AlunoId")?.Value;

        if (role == "Aluno" && alunoIdClaim != alunoId.ToString())
        {
            return Forbid();
        }

        var sucesso = await _notificationService.MarcarComoLidaAsync(id, alunoId);
        if (!sucesso) return NotFound();

        return NoContent();
    }
}
