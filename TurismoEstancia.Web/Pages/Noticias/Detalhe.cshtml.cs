using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Comunicacao.Interfaces;

namespace TurismoEstancia.Web.Pages.Noticias;

public class DetalheModel : PageModel
{
    private readonly INoticiaService _noticias;

    public DetalheModel(INoticiaService noticias) => _noticias = noticias;

    public NoticiaDto? Noticia { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
    {
        Noticia = await _noticias.ObterPorSlugAsync(slug, ct);
        return Noticia is null ? NotFound() : Page();
    }
}
