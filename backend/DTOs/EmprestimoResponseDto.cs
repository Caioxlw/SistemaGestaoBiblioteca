namespace BibliotecaAPI.DTOs;

public class EmprestimoResponseDto
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public string NomeAluno { get; set; } = string.Empty;
    public int LivroId { get; set; }
    public string TituloLivro { get; set; } = string.Empty;
    public DateTime DataEmprestimo { get; set; }
    public DateTime DataPrevistaDevolucao { get; set; }
    public DateTime? DataDevolucao { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DiasAtraso { get; set; }
    public decimal Multa { get; set; }
}