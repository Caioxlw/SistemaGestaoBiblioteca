using System;

namespace BibliotecaAPI.Models;

public class LogAuditoria
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty; // Criou, Atualizou, Excluiu
    public string Entidade { get; set; } = string.Empty; // Livro, Aluno, etc
    public int? EntidadeId { get; set; }
    public string Detalhes { get; set; } = string.Empty;
}
