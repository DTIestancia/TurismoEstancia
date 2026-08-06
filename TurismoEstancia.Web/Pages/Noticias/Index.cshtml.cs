using Microsoft.AspNetCore.Mvc.RazorPages;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Pages.Noticias;

public class IndexModel : PageModel
{
    private readonly INoticiaService _noticias;

    public IndexModel(INoticiaService noticias) => _noticias = noticias;

    public IReadOnlyList<NoticiaDto> Noticias { get; private set; } = Array.Empty<NoticiaDto>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Noticias = await _noticias.ListarAsync(apenasPublicadas: true, ct);
        ViewData["Seo"] = new SeoMeta
        {
            Titulo = "Notícias",
            Descricao = "Cultura, eventos e novidades de Estância — Capital Sergipana da Cultura."
        };
    }
}
