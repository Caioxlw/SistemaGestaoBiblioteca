using BibliotecaAPI.DTOs;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BibliotecaAPI.Services;

public class LivroService : ILivroService
{
    private readonly ILivroRepository _livroRepository;
    private readonly IAutorRepository _autorRepository;
    private readonly IAuditoriaService? _auditoriaService;
    private readonly ICacheService? _cacheService;

    public LivroService(
        ILivroRepository livroRepository, 
        IAutorRepository autorRepository,
        IAuditoriaService? auditoriaService = null,
        ICacheService? cacheService = null)
    {
        _livroRepository = livroRepository;
        _autorRepository = autorRepository;
        _auditoriaService = auditoriaService;
        _cacheService = cacheService;
    }

    public async Task<LivroResponseDto> CriarAsync(CriarLivroDto dto)
    {
        var autor = await _autorRepository.ObterPorIdAsync(dto.AutorId);
        if (autor == null) throw new ArgumentException("Autor não encontrado");

        int ano = dto.Ano.HasValue && dto.Ano.Value > 0 ? dto.Ano.Value : dto.AnoPublicacao;

        var livro = new Livro
        {
            Isbn = dto.Isbn,
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            AnoPublicacao = ano,
            Editora = dto.Editora,
            Categoria = dto.Categoria,
            Quantidade = dto.Quantidade,
            Localizacao = dto.Localizacao,
            AutorId = dto.AutorId
        };

        await _livroRepository.AdicionarAsync(livro);

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                nomeUsuario: string.Empty,
                acao: "Criou Livro",
                entidade: "Livro",
                entidadeId: livro.Id,
                detalhes: $"Livro '{livro.Titulo}' (ISBN: {livro.Isbn}) cadastrado com sucesso."
            );
        }

        if (_cacheService != null)
        {
            await _cacheService.RemoveAsync("livros:populares");
        }

        return MapearParaDto(livro);
    }

    public async Task<PagedResult<LivroResponseDto>> ObterTodosAsync(string? termo, int page, int pageSize)
    {
        var livros = await _livroRepository.ObterTodosAsync(null, null);
        
        if (!string.IsNullOrWhiteSpace(termo))
        {
            termo = termo.ToLower();
            livros = livros.Where(l => l.Titulo.ToLower().Contains(termo) || 
                                       l.Isbn.Contains(termo) ||
                                       (l.Autor?.Nome.ToLower().Contains(termo) ?? false));
        }

        int totalItens = livros.Count();
        int totalPaginas = (int)Math.Ceiling(totalItens / (double)pageSize);

        var itensPaginados = livros
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapearParaDto)
            .ToList();

        return new PagedResult<LivroResponseDto>
        {
            Itens = itensPaginados,
            PaginaAtual = page,
            TotalPaginas = totalPaginas,
            TotalItens = totalItens
        };
    }

    public async Task<LivroResponseDto> ObterPorIdAsync(int id)
    {
        var livro = await _livroRepository.ObterPorIdAsync(id);
        if (livro == null) throw new ArgumentException("Livro não encontrado");
        return MapearParaDto(livro);
    }

    public async Task<LivroResponseDto> AtualizarAsync(int id, CriarLivroDto dto)
    {
        var livro = await _livroRepository.ObterPorIdAsync(id);
        if (livro == null) throw new ArgumentException("Livro não encontrado");

        var autor = await _autorRepository.ObterPorIdAsync(dto.AutorId);
        if (autor == null) throw new ArgumentException("Autor não encontrado");

        int ano = dto.Ano.HasValue && dto.Ano.Value > 0 ? dto.Ano.Value : dto.AnoPublicacao;

        livro.Isbn = dto.Isbn;
        livro.Titulo = dto.Titulo;
        livro.Descricao = dto.Descricao;
        livro.AnoPublicacao = ano;
        livro.Editora = dto.Editora;
        livro.Categoria = dto.Categoria;
        livro.Quantidade = dto.Quantidade;
        livro.Localizacao = dto.Localizacao;
        livro.AutorId = dto.AutorId;

        await _livroRepository.AtualizarAsync(livro);

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                nomeUsuario: string.Empty,
                acao: "Atualizou Livro",
                entidade: "Livro",
                entidadeId: livro.Id,
                detalhes: $"Livro '{livro.Titulo}' atualizado com sucesso."
            );
        }

        if (_cacheService != null)
        {
            await _cacheService.RemoveAsync("livros:populares");
        }

        return MapearParaDto(livro);
    }

    public async Task ExcluirAsync(int id)
    {
        await _livroRepository.ExcluirAsync(id);

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                nomeUsuario: string.Empty,
                acao: "Excluiu Livro",
                entidade: "Livro",
                entidadeId: id,
                detalhes: $"Livro com ID {id} foi excluído do acervo."
            );
        }

        if (_cacheService != null)
        {
            await _cacheService.RemoveAsync("livros:populares");
        }
    }

    private LivroResponseDto MapearParaDto(Livro livro)
    {
        return new LivroResponseDto
        {
            Id = livro.Id,
            Isbn = livro.Isbn,
            Titulo = livro.Titulo,
            Descricao = livro.Descricao,
            AnoPublicacao = livro.AnoPublicacao,
            Editora = livro.Editora,
            Categoria = livro.Categoria,
            Quantidade = livro.Quantidade,
            Localizacao = livro.Localizacao,
            AutorId = livro.AutorId,
            NomeAutor = livro.Autor?.Nome ?? ""
        };
    }
}