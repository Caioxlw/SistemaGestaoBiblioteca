using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/alunos")]
public class AlunosController : ControllerBase
{
    private readonly IAlunoService _alunoService;

    public AlunosController(IAlunoService alunoService)
    {
        _alunoService = alunoService;
    }

    [HttpPost]
    public async Task<ActionResult<AlunoResponseDto>> Criar([FromBody] CriarAlunoDto dto)
    {
        var aluno = await _alunoService.CriarAsync(dto);
        return StatusCode(201, aluno);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AlunoResponseDto>>> ObterTodos()
    {
        var alunos = await _alunoService.ObterTodosAsync();
        return Ok(alunos);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AlunoResponseDto>> Atualizar(int id, [FromBody] CriarAlunoDto dto)
    {
        var aluno = await _alunoService.AtualizarAsync(id, dto);
        return Ok(aluno);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Excluir(int id)
    {
        await _alunoService.ExcluirAsync(id);
        return NoContent();
    }
}