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
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    [HttpGet("fila/{livroId:int}")]
    [Authorize(Roles = "Admin,Bibliotecario")]
    public async Task<ActionResult<IEnumerable<ReservaResponseDto>>> ObterFila(int livroId)
    {
        var fila = await _reservaService.ObterFilaReservaAsync(livroId);
        return Ok(fila);
    }

    [HttpGet("aluno/{alunoId:int}")]
    [Authorize(Roles = "Admin,Bibliotecario,Aluno")]
    public async Task<ActionResult<IEnumerable<ReservaResponseDto>>> ObterPorAluno(int alunoId)
    {
        // Alunos só podem ver suas próprias reservas, bibliotecários e admins podem ver de todos
        var userIdClaim = User.FindFirst("AlunoId")?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (role == "Aluno" && userIdClaim != alunoId.ToString())
        {
            return Forbid();
        }

        var reservas = await _reservaService.ObterReservasAlunoAsync(alunoId);
        return Ok(reservas);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Bibliotecario,Aluno")]
    public async Task<ActionResult<ReservaResponseDto>> Criar([FromBody] CriarReservaDto dto)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var alunoIdClaim = User.FindFirst("AlunoId")?.Value;

        // Se for aluno, garante que só pode reservar em seu próprio nome
        if (role == "Aluno" && alunoIdClaim != dto.AlunoId.ToString())
        {
            return Forbid();
        }

        var reserva = await _reservaService.CriarReservaAsync(dto);
        return Ok(reserva);
    }
}
