namespace BibliotecaAPI.Models;

public enum StatusEmprestimo
{
    Ativo = 0,
    Devolvido = 1,
    Atrasado = 2
}
public class Emprestimo
{
    public int Id {get; set;}
    public Aluno? Aluno {get; set;}
    public int LivroId {get; set;}
    public Livro? Livro {get; set;}
    public DateTime DataEmprestimo { get; set; }
    public DateTime DataPrevistaDevolucao { get; set; }
    public DateTime? DataDevolucao { get; set; }

    public StatusEmprestimo Status {get; set;} = StatusEmprestimo.Ativo;
}