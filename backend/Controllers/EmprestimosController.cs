using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/emprestimos")]
public class EmprestimosController : ControllerBase
{
    private readonly IEmprestimoService _emprestimoService;

    public EmprestimosController(IEmprestimoService emprestimoService)
    {
        _emprestimoService = emprestimoService;
    }

    [HttpPost]
    public async Task<ActionResult<EmprestimoResponseDto>> Criar([FromBody] CriarEmprestimoDto dto)
    {
        var emprestimo = await _emprestimoService.CriarAsync(dto);
        return StatusCode(201, emprestimo);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmprestimoResponseDto>>> ObterTodos()
    {
        var emprestimos = await _emprestimoService.ObterTodosAsync();
        return Ok(emprestimos);
    }

    [HttpGet("abertos")]
    public async Task<ActionResult<IEnumerable<EmprestimoResponseDto>>> ObterAbertos()
    {
        var emprestimos = await _emprestimoService.ObterAbertosAsync();
        return Ok(emprestimos);
    }

    [HttpGet("aluno/{alunoId:int}")]
    public async Task<ActionResult<IEnumerable<EmprestimoResponseDto>>> ObterPorAluno(int alunoId)
    {
        var emprestimos = await _emprestimoService.ObterPorAlunoAsync(alunoId);
        return Ok(emprestimos);
    }

    [HttpPut("{id:int}/devolucao")]
    public async Task<ActionResult<EmprestimoResponseDto>> Devolucao(int id)
    {
        var emprestimo = await _emprestimoService.DevolverAsync(id);
        return Ok(emprestimo);
    }
}