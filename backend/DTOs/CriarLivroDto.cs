using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class CriarLivroDto
{
    [Required(ErrorMessage = "O ISBN é obrigatório.")]
    public string Isbn { get; set; } = string.Empty;

    [Required(ErrorMessage = "O título é obrigatório.")]
    public string Titulo { get; set; } = string.Empty;

    [Range(1000, 2100, ErrorMessage = "Ano de publicação inválido.")]
    public int AnoPublicacao { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "A quantidade não pode ser negativa.")]
    public int Quantidade { get; set; }

    [Required(ErrorMessage = "O ID do autor é obrigatório.")]
    public int AutorId { get; set; }
}