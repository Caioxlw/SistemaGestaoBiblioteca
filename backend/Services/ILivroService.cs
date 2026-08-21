using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Services;

public interface ILivroService
{
    Task<LivroResponseDto> CriarAsync(CriarLivroDto dto);
    Task<IEnumerable<LivroResponseDto>> ObterTodosAsync(string? titulo, string? autor);
    Task<LivroResponseDto> AtualizarAsync(int id, CriarLivroDto dto);
    Task ExcluirAsync(int id);
    Task<LivroResponseDto> ObterPorIdAsync(int id);
}