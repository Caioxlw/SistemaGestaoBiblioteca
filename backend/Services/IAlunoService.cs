using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Services;

public interface IAlunoService
{
    Task<AlunoResponseDto> CriarAsync(CriarAlunoDto dto);
    Task<IEnumerable<AlunoResponseDto>> ObterTodosAsync();
    Task<AlunoResponseDto> AtualizarAsync(int id, CriarAlunoDto dto);
    Task ExcluirAsync(int id);
}