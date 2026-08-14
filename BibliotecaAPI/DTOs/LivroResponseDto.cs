namespace BibliotecaAPI.DTOs;

public class LivroResponseDto
{
    public int Id { get; set; }
    public string Isbn { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public int AnoPublicacao { get; set; }
    public int Quantidade { get; set; }
    public int AutorId { get; set; }
    public string NomeAutor { get; set; } = string.Empty;
}