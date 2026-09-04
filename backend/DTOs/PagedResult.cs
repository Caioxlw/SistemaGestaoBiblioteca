using System.Collections.Generic;

namespace BibliotecaAPI.DTOs;

public class PagedResult<T>
{
    public IEnumerable<T> Itens { get; set; } = new List<T>();
    public int PaginaAtual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalItens { get; set; }
}
