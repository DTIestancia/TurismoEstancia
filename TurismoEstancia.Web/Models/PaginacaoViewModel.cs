namespace TurismoEstancia.Web.Models;

/// <summary>
/// Paginação reutilizável (partial <c>_Paginacao</c>). <c>UrlBase</c> é o caminho
/// sem o parâmetro de página (ex.: "/galeria", "/galeria/praia-do-saco",
/// "/Gerenciador/Noticias") — o partial anexa "?pagina={n}".
/// </summary>
public class PaginacaoViewModel
{
    public int PaginaAtual { get; set; } = 1;

    public int TotalPaginas { get; set; } = 1;

    public string? UrlBase { get; set; }

    /// <summary>Nome do parâmetro de página na URL (default "pagina"). Use outro valor
    /// (ex.: "paginaPassados") quando a página tiver mais de uma listagem paginada.</summary>
    public string Parametro { get; set; } = "pagina";

    /// <summary>Números de página a exibir (janela ao redor da atual); -1 = reticências.</summary>
    public IReadOnlyList<int> Paginas
    {
        get
        {
            var total = Math.Max(TotalPaginas, 1);
            if (total <= 7)
                return Enumerable.Range(1, total).ToList();

            var atual = Math.Clamp(PaginaAtual, 1, total);
            var inicio = Math.Max(1, Math.Min(atual - 2, total - 4));
            var fim = Math.Min(total, inicio + 4);

            var lista = new List<int>();
            if (inicio > 1) lista.Add(1);
            if (inicio > 2) lista.Add(-1);
            for (var i = inicio; i <= fim; i++) lista.Add(i);
            if (fim < total - 1) lista.Add(-1);
            if (fim < total) lista.Add(total);
            return lista;
        }
    }

    public static string Url(int pagina, string? urlBase, string parametro = "pagina") =>
        $"{urlBase}{(urlBase is not null && urlBase.Contains('?') ? '&' : '?')}{parametro}={pagina}";
}
