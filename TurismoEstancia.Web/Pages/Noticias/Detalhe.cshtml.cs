using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Pages.Noticias;

public class DetalheModel : PageModel
{
    private readonly INoticiaService _noticias;

    public DetalheModel(INoticiaService noticias) => _noticias = noticias;

    public NoticiaDto? Noticia { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
    {
        Noticia = await _noticias.ObterPorSlugAsync(slug, ct);
        if (Noticia is null) return NotFound();

        ViewData["Seo"] = new SeoMeta
        {
            Titulo = Noticia.Titulo,
            // Sem tags HTML no meta description: usa o resumo ou o corpo sem marcação.
            Descricao = Noticia.Resumo ?? RemoverTagsHtml(Noticia.Corpo),
            ImagemUrl = Noticia.ImagemArquivoId is long img ? $"/arquivo/{img}" : null,
            Tipo = "article",
            DataPublicacao = Noticia.DataPublicacao.ToString("o")
        };
        return Page();
    }

    /// <summary>Remove marcação HTML do texto (para meta description).</summary>
    private static string RemoverTagsHtml(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";
        return System.Text.RegularExpressions.Regex.Replace(texto, "<[^>]*>", " ").Trim();
    }
}
