using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class CriarAutorDto
{
    [Required(ErrorMessage = "O nome do autor é obrigatório.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
    public DateTime DataNascimento { get; set; }

    [Required(ErrorMessage = "A nacionalidade é obrigatória.")]
    public string Nacionalidade { get; set; } = string.Empty;
}