using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Services;

public interface ILivroService
{
    Task<IEnumerable<LivroResponseDto>> ObterTodosAsync(string? titulo, string? autor);
    Task<LivroResponseDto> ObterPorIdAsync(int id);
    Task<LivroResponseDto> CriarAsync(CriarLivroDto dto);
}