using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;

namespace BibliotecaAPI.Services;

public class LivroService : ILivroService
{
    private readonly ILivroRepository _livroRepository;
    private readonly IAutorRepository _autorRepository;

    public LivroService(ILivroRepository livroRepository, IAutorRepository autorRepository)
    {
        _livroRepository = livroRepository;
        _autorRepository = autorRepository;
    }

    public async Task<IEnumerable<LivroResponseDto>> ObterTodosAsync(string? titulo, string? autor)
    {
        var livros = await _livroRepository.ObterTodosAsync(titulo, autor);
        return livros.Select(l => new LivroResponseDto
        {
            Id = l.Id,
            Isbn = l.Isbn,
            Titulo = l.Titulo,
            AnoPublicacao = l.AnoPublicacao,
            Quantidade = l.Quantidade,
            AutorId = l.AutorId,
            NomeAutor = l.Autor?.Nome ?? string.Empty
        });
    }

    public async Task<LivroResponseDto> ObterPorIdAsync(int id)
    {
        var livro = await _livroRepository.ObterPorIdAsync(id)
            ?? throw new NotFoundException($"Livro com ID {id} não foi encontrado.");

        return new LivroResponseDto
        {
            Id = livro.Id,
            Isbn = livro.Isbn,
            Titulo = livro.Titulo,
            AnoPublicacao = livro.AnoPublicacao,
            Quantidade = livro.Quantidade,
            AutorId = livro.AutorId,
            NomeAutor = livro.Autor?.Nome ?? string.Empty
        };
    }

    public async Task<LivroResponseDto> CriarAsync(CriarLivroDto dto)
    {
        var autor = await _autorRepository.ObterPorIdAsync(dto.AutorId)
            ?? throw new NotFoundException($"Autor com ID {dto.AutorId} não existe.");

        var livro = new Livro
        {
            Isbn = dto.Isbn,
            Titulo = dto.Titulo,
            AnoPublicacao = dto.AnoPublicacao,
            Quantidade = dto.Quantidade,
            AutorId = dto.AutorId
        };

        await _livroRepository.AdicionarAsync(livro);

        return new LivroResponseDto
        {
            Id = livro.Id,
            Isbn = livro.Isbn,
            Titulo = livro.Titulo,
            AnoPublicacao = livro.AnoPublicacao,
            Quantidade = livro.Quantidade,
            AutorId = livro.AutorId,
            NomeAutor = autor.Nome
        };
    }

    public async Task<LivroResponseDto> AtualizarAsync(int id, CriarLivroDto dto)
    {
        var livro = await _livroRepository.ObterPorIdAsync(id)
            ?? throw new NotFoundException($"Livro com ID {id} não encontrado.");

        var autor = await _autorRepository.ObterPorIdAsync(dto.AutorId)
            ?? throw new NotFoundException($"Autor com ID {dto.AutorId} não encontrado.");

        livro.Titulo = dto.Titulo;
        livro.Isbn = dto.Isbn;
        livro.AnoPublicacao = dto.AnoPublicacao;
        livro.Quantidade = dto.Quantidade;
        livro.AutorId = dto.AutorId;
        livro.Autor = autor;

        await _livroRepository.AtualizarAsync(livro);

        return new LivroResponseDto
        {
            Id = livro.Id,
            Titulo = livro.Titulo,
            Isbn = livro.Isbn,
            AnoPublicacao = livro.AnoPublicacao,
            Quantidade = livro.Quantidade,
            AutorId = livro.AutorId,
            NomeAutor = autor.Nome
        };
    }

    public async Task ExcluirAsync(int id)
    {
        var livro = await _livroRepository.ObterPorIdAsync(id)
            ?? throw new NotFoundException($"Livro com ID {id} não encontrado.");
            
        await _livroRepository.ExcluirAsync(id);
    }
}