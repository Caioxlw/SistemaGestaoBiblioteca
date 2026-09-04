using BibliotecaAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BibliotecaAPI.Services;

public interface ILivroService
{
    Task<LivroResponseDto> CriarAsync(CriarLivroDto dto);
    Task<PagedResult<LivroResponseDto>> ObterTodosAsync(string? termo, int page, int pageSize);
    Task<LivroResponseDto> ObterPorIdAsync(int id);
    Task<LivroResponseDto> AtualizarAsync(int id, CriarLivroDto dto);
    Task ExcluirAsync(int id);
}