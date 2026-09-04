using System;

namespace BibliotecaAPI.Models;

public class Reserva
{
    public int Id { get; set; }
    public int LivroId { get; set; }
    public Livro? Livro { get; set; }
    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }
    public DateTime DataReserva { get; set; }
    public string Status { get; set; } = "Pendente"; // Pendente, Atendida, Cancelada
}
