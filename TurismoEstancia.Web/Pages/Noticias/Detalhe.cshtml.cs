using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.Galeria.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Pages.Noticias;

public class DetalheModel : PageModel
{
    private readonly INoticiaService _noticias;
    private readonly IGaleriaService _galeria;

    public DetalheModel(INoticiaService noticias, IGaleriaService galeria)
    {
        _noticias = noticias;
        _galeria = galeria;
    }

    public NoticiaDto? Noticia { get; private set; }

    /// <summary>Galeria relacionada à notícia (quando vinculada no painel).</summary>
    public GaleriaCategoriaDto? Galeria { get; private set; }

    /// <summary>Fotos ativas da galeria relacionada (até 8, para a faixa da notícia).</summary>
    public IReadOnlyList<GaleriaMidiaDto> GaleriaFotos { get; private set; } = Array.Empty<GaleriaMidiaDto>();

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
    {
        Noticia = await _noticias.ObterPorSlugAsync(slug, ct);
        if (Noticia is null) return NotFound();

        // Galeria relacionada (opcional): carrega a categoria + as fotos ativas.
        if (Noticia.GaleriaCategoriaId is int galId)
        {
            Galeria = await _galeria.ObterCategoriaPorIdAsync(galId, ct);
            if (Galeria is { Ativo: true })
            {
                GaleriaFotos = await _galeria.ListarFotosAsync(galId, apenasAtivos: true, ct);
            }
        }

        ViewData["Seo"] = new SeoMeta
        {
            Titulo = Noticia.Titulo,
            // Sem tags HTML no meta description: usa o resumo ou o corpo sem marcação.
            Descricao = Noticia.Resumo ?? RemoverTagsHtml(Noticia.Corpo),
            ImagemUrl = Noticia.ImagemArquivoId is long img ? Request.PathBase + $"/arquivo/{img}" : null,
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
