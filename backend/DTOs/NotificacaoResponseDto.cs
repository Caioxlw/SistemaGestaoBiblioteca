using System;

namespace BibliotecaAPI.DTOs;

public class NotificacaoResponseDto
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public string NomeAluno { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public DateTime DataNotificacao { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public bool Lida { get; set; }
}
