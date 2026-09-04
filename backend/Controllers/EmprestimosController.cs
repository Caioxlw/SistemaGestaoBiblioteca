using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/emprestimos")]
[Authorize]
public class EmprestimosController : ControllerBase
{
    private readonly IEmprestimoService _emprestimoService;

    public EmprestimosController(IEmprestimoService emprestimoService)
    {
        _emprestimoService = emprestimoService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Bibliotecario")]
    public async Task<ActionResult<EmprestimoResponseDto>> Criar([FromBody] CriarEmprestimoDto dto)
    {
        var emprestimo = await _emprestimoService.CriarAsync(dto);
        return StatusCode(201, emprestimo);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Bibliotecario")]
    public async Task<ActionResult<IEnumerable<EmprestimoResponseDto>>> ObterTodos()
    {
        var emprestimos = await _emprestimoService.ObterTodosAsync();
        return Ok(emprestimos);
    }

    [HttpGet("abertos")]
    [Authorize(Roles = "Admin,Bibliotecario")]
    public async Task<ActionResult<IEnumerable<EmprestimoResponseDto>>> ObterAbertos()
    {
        var emprestimos = await _emprestimoService.ObterAbertosAsync();
        return Ok(emprestimos);
    }

    [HttpGet("aluno/{alunoId:int}")]
    [Authorize(Roles = "Admin,Bibliotecario,Aluno")]
    public async Task<ActionResult<IEnumerable<EmprestimoResponseDto>>> ObterPorAluno(int alunoId)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var alunoIdClaim = User.FindFirst("AlunoId")?.Value;

        if (role == "Aluno" && alunoIdClaim != alunoId.ToString())
        {
            return Forbid();
        }

        var emprestimos = await _emprestimoService.ObterPorAlunoAsync(alunoId);
        return Ok(emprestimos);
    }

    /// <summary>
    /// Devolução via PUT /api/emprestimos/{id}/devolucao (utilizada no frontend).
    /// </summary>
    [HttpPut("{id:int}/devolucao")]
    [Authorize(Roles = "Admin,Bibliotecario")]
    public async Task<ActionResult<EmprestimoResponseDto>> Devolucao(int id)
    {
        var emprestimo = await _emprestimoService.DevolverAsync(id);
        return Ok(emprestimo);
    }

    /// <summary>
    /// Devolução via POST /api/emprestimos/devolver (conforme especificação dos requisitos).
    /// </summary>
    [HttpPost("devolver")]
    [Authorize(Roles = "Admin,Bibliotecario")]
    public async Task<ActionResult<EmprestimoResponseDto>> Devolver([FromBody] DevolverEmprestimoDto? dto, [FromQuery] int? id)
    {
        int emprestimoId = dto?.EmprestimoId ?? id ?? 0;
        if (emprestimoId <= 0)
        {
            return BadRequest(new { message = "Informe o ID do empréstimo para efetuar a devolução." });
        }

        var emprestimo = await _emprestimoService.DevolverAsync(emprestimoId);
        return Ok(emprestimo);
    }
}