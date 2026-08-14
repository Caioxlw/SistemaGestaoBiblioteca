using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Services;

public interface IAutorService
{
    Task<IEnumerable<AutorResponseDto>> ObterTodosAsync();
    Task<AutorResponseDto> ObterPorIdAsync(int id);
    Task<AutorResponseDto> CriarAsync(CriarAutorDto dto);
}