using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class CriarLivroDto
{
    [Required(ErrorMessage = "O ISBN é obrigatório.")]
    public string Isbn { get; set; } = string.Empty;

    [Required(ErrorMessage = "O título é obrigatório.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    public string Descricao { get; set; } = string.Empty;

    [Range(1000, 2100, ErrorMessage = "Ano de publicação inválido.")]
    public int AnoPublicacao { get; set; }

    public int? Ano
    {
        get => AnoPublicacao;
        set { if (value.HasValue && value.Value > 0) AnoPublicacao = value.Value; }
    }

    [Required(ErrorMessage = "A editora é obrigatória.")]
    public string Editora { get; set; } = string.Empty;

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public string Categoria { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "A quantidade não pode ser negativa.")]
    public int Quantidade { get; set; }
    
    [Required(ErrorMessage = "A localização é obrigatória.")]
    public string Localizacao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O ID do autor é obrigatório.")]
    public int AutorId { get; set; }
}