using System;

namespace BibliotecaAPI.DTOs;

public class ReservaResponseDto
{
    public int Id { get; set; }
    public int LivroId { get; set; }
    public string TituloLivro { get; set; } = string.Empty;
    public int AlunoId { get; set; }
    public string NomeAluno { get; set; } = string.Empty;
    public DateTime DataReserva { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CriarReservaDto
{
    public int LivroId { get; set; }
    public int AlunoId { get; set; }
}
