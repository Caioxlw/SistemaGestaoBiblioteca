namespace BibliotecaAPI.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }
    
    // Relação opcional com Aluno (se o usuário for do tipo Aluno)
    public int? AlunoId { get; set; }
    public Aluno? Aluno { get; set; }
}

public enum PerfilUsuario
{
    Admin,
    Bibliotecario,
    Aluno
}
