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
}