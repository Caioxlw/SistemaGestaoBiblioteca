using BibliotecaAPI.DTOs;
using BibliotecaAPI.Exceptions;
using BibliotecaAPI.Models;
using BibliotecaAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BibliotecaAPI.Services;

public class AutorService : IAutorService
{
    private readonly IAutorRepository _autorRepository;
    private readonly IAuditoriaService? _auditoriaService;

    public AutorService(IAutorRepository autorRepository, IAuditoriaService? auditoriaService = null)
    {
        _autorRepository = autorRepository;
        _auditoriaService = auditoriaService;
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

        var dataNascUtc = dto.DataNascimento.Kind == DateTimeKind.Utc 
            ? dto.DataNascimento 
            : DateTime.SpecifyKind(dto.DataNascimento, DateTimeKind.Utc);

        autor.Nome = dto.Nome;
        autor.Nacionalidade = dto.Nacionalidade;
        autor.DataNascimento = dataNascUtc;

        await _autorRepository.AtualizarAsync(autor);

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                nomeUsuario: string.Empty,
                acao: "Atualizou Autor",
                entidade: "Autor",
                entidadeId: autor.Id,
                detalhes: $"Autor '{autor.Nome}' atualizado com sucesso."
            );
        }

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
            
        await _autorRepository.ExcluirAsync(id);

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                nomeUsuario: string.Empty,
                acao: "Excluiu Autor",
                entidade: "Autor",
                entidadeId: id,
                detalhes: $"Autor '{autor.Nome}' (ID: {id}) foi excluído."
            );
        }
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
        var dataNascUtc = dto.DataNascimento.Kind == DateTimeKind.Utc 
            ? dto.DataNascimento 
            : DateTime.SpecifyKind(dto.DataNascimento, DateTimeKind.Utc);

        var autor = new Autor
        {
            Nome = dto.Nome,
            DataNascimento = dataNascUtc,
            Nacionalidade = dto.Nacionalidade
        };

        await _autorRepository.AdicionarAsync(autor);

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                nomeUsuario: string.Empty,
                acao: "Cadastrou Autor",
                entidade: "Autor",
                entidadeId: autor.Id,
                detalhes: $"Autor '{autor.Nome}' cadastrado com sucesso."
            );
        }

        return new AutorResponseDto
        {
            Id = autor.Id,
            Nome = autor.Nome,
            DataNascimento = autor.DataNascimento,
            Nacionalidade = autor.Nacionalidade
        };
    }
}