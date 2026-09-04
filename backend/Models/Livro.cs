namespace BibliotecaAPI.Models;

public class Livro
{
    public int Id {get; set;}
    public string Isbn {get; set;} = string.Empty;
    public string Titulo {get; set;} = string.Empty;
    public string Descricao {get; set;} = string.Empty;
    public int AnoPublicacao {get; set;}
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int Ano { get => AnoPublicacao; set => AnoPublicacao = value; }
    public string Editora {get; set;} = string.Empty;
    public string Categoria {get; set;} = string.Empty;
    public int Quantidade {get; set;}
    public string Localizacao {get; set;} = string.Empty;
    
    public int AutorId {get; set;}
    public Autor? Autor {get; set;}
}