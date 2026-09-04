using System;

namespace BibliotecaAPI.DTOs;

public class AuditoriaDto
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty;
    public string Entidade { get; set; } = string.Empty;
    public int? EntidadeId { get; set; }
    public string Detalhes { get; set; } = string.Empty;
}
