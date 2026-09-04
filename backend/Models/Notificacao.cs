using System;

namespace BibliotecaAPI.Models;

public class Notificacao
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public DateTime DataNotificacao { get; set; } = DateTime.UtcNow;
    public string Tipo { get; set; } = "ReservaDisponivel";
    public bool Lida { get; set; } = false;
}
