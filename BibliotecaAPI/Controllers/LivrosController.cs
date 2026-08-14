using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/livros")]
public class LivrosController : ControllerBase
{
    private readonly ILivroService _livroService;

    public LivrosController(ILivroService livroService)
    {
        _livroService = livroService;
    }

    [HttpPost]
    public async Task<ActionResult<LivroResponseDto>> Criar([FromBody] CriarLivroDto dto)
    {
        var livro = await _livroService.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = livro.Id }, livro);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LivroResponseDto>>> ObterTodos(
        [FromQuery] string? titulo, 
        [FromQuery] string? autor)
    {
        var livros = await _livroService.ObterTodosAsync(titulo, autor);
        return Ok(livros);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LivroResponseDto>> ObterPorId(int id)
    {
        var livro = await _livroService.ObterPorIdAsync(id);
        return Ok(livro);
    }
}