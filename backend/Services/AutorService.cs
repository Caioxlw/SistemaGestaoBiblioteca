using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;

namespace BibliotecaAPI.Services;

public class AutorService : IAutorService
{
    private readonly IAutorRepository _autorRepository;

    public AutorService(IAutorRepository autorRepository)
    {
        _autorRepository = autorRepository;
    }

    public async Task<IEnumerable<AutorResponseDto>> ObterTodosAsync()
    {
        var autores = await _autorRepository.ObterTodosAsync();
        return autores.Select(a => new AutorResponseDto
        {
            Id = a.Id,
            Nome = a.Nome,
            Nacionalidade = a.Nacionalidade,
            DataNascimento = a.DataNascimento
        });
    }

    public async Task<AutorResponseDto> AtualizarAsync(int id, CriarAutorDto dto)
    {
        var autor = await _autorRepository.ObterPorIdAsync(id) 
            ?? throw new NotFoundException($"Autor com ID {id} não encontrado.");

        autor.Nome = dto.Nome;
        autor.Nacionalidade = dto.Nacionalidade;
        autor.DataNascimento = dto.DataNascimento;

        await _autorRepository.AtualizarAsync(autor);

        return new AutorResponseDto
        {
            Id = autor.Id,
            Nome = autor.Nome,
            Nacionalidade = autor.Nacionalidade,
            DataNascimento = autor.DataNascimento
        };
    }

    public async Task ExcluirAsync(int id)
    {
        var autor = await _autorRepository.ObterPorIdAsync(id)
            ?? throw new NotFoundException($"Autor com ID {id} não encontrado.");
            
        // Regra de negócio: não excluir se tiver livros. Opcional, mas o SQLite vai dar FK Error.
        // Vamos deixar o banco estourar ou implementar depois se necessário.
        await _autorRepository.ExcluirAsync(id);
    }

    public async Task<AutorResponseDto> ObterPorIdAsync(int id)
    {
        var autor = await _autorRepository.ObterPorIdAsync(id)
            ?? throw new NotFoundException($"Autor com ID {id} não foi encontrado.");

        return new AutorResponseDto
        {
            Id = autor.Id,
            Nome = autor.Nome,
            DataNascimento = autor.DataNascimento,
            Nacionalidade = autor.Nacionalidade
        };
    }

    public async Task<AutorResponseDto> CriarAsync(CriarAutorDto dto)
    {
        var autor = new Autor
        {
            Nome = dto.Nome,
            DataNascimento = dto.DataNascimento,
            Nacionalidade = dto.Nacionalidade
        };

        await _autorRepository.AdicionarAsync(autor);

        return new AutorResponseDto
        {
            Id = autor.Id,
            Nome = autor.Nome,
            DataNascimento = autor.DataNascimento,
            Nacionalidade = autor.Nacionalidade
        };
    }
}