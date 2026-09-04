using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class CriarEmprestimoDto
{
    [Required(ErrorMessage = "O ID do aluno é obrigatório.")]
    public int AlunoId { get; set; }

    [Required(ErrorMessage = "O ID do livro é obrigatório.")]
    public int LivroId { get; set; }
}