using BibliotecaAPI.DTOs;
using BibliotecaAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/livros")]
[Authorize(Roles = "Admin,Bibliotecario")]
public class LivrosController : ControllerBase
{
    private readonly ILivroService _livroService;
    private readonly IDashboardService _dashboardService;

    public LivrosController(ILivroService livroService, IDashboardService dashboardService)
    {
        _livroService = livroService;
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Endpoint oficial de Livros Populares com Cache-Aside (Redis).
    /// </summary>
    [HttpGet("populares")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RelatorioPopularDto>>> ObterPopulares()
    {
        var populares = await _dashboardService.ObterLivrosMaisPopularesAsync();
        return Ok(populares);
    }

    [HttpPost]
    public async Task<ActionResult<LivroResponseDto>> Criar([FromBody] CriarLivroDto dto)
    {
        var livro = await _livroService.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = livro.Id }, livro);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<LivroResponseDto>>> ObterTodos(
        [FromQuery] string? termo, 
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var resultado = await _livroService.ObterTodosAsync(termo, page, pageSize);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<LivroResponseDto>> ObterPorId(int id)
    {
        var livro = await _livroService.ObterPorIdAsync(id);
        return Ok(livro);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LivroResponseDto>> Atualizar(int id, [FromBody] CriarLivroDto dto)
    {
        var livro = await _livroService.AtualizarAsync(id, dto);
        return Ok(livro);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Excluir(int id)
    {
        await _livroService.ExcluirAsync(id);
        return NoContent();
    }
}