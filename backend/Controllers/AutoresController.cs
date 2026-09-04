using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/autores")]
[Authorize(Roles = "Admin,Bibliotecario")]
public class AutoresController : ControllerBase
{
    private readonly IAutorService _autorService;

    public AutoresController(IAutorService autorService)
    {
        _autorService = autorService;
    }

    [HttpPost]
    public async Task<ActionResult<AutorResponseDto>> Criar([FromBody] CriarAutorDto dto)
    {
        var autor = await _autorService.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = autor.Id }, autor);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<AutorResponseDto>>> ObterTodos()
    {
        var autores = await _autorService.ObterTodosAsync();
        return Ok(autores);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<AutorResponseDto>> ObterPorId(int id)
    {
        var autor = await _autorService.ObterPorIdAsync(id);
        return Ok(autor);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AutorResponseDto>> Atualizar(int id, [FromBody] CriarAutorDto dto)
    {
        var autor = await _autorService.AtualizarAsync(id, dto);
        return Ok(autor);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Excluir(int id)
    {
        await _autorService.ExcluirAsync(id);
        return NoContent();
    }
}