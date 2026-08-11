using Microsoft.AspNetCore.Mvc;
using System.Xml;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;
using TurismoEstancia.Services.Galeria.Interfaces;
using TurismoEstancia.Services.Roteiro.Interfaces;
using TurismoEstancia.Services.Turismo.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Controllers;

/// <summary>
/// SEO do portal: /sitemap.xml (rotas públicas + páginas de detalhe do banco)
/// e /robots.txt. Tudo é gerado dinamicamente a partir do conteúdo real.
/// </summary>
public class SeoController : Controller
{
    private readonly IPontoTuristicoService _pontos;
    private readonly INoticiaService _noticias;
    private readonly IRoteiroService _roteiros;
    private readonly IGrupoCulturalService _grupos;
    private readonly IPratoTuristicoService _pratos;
    private readonly ITagCulturalService _tags;
    private readonly IGaleriaService _galeria;

    public SeoController(
        IPontoTuristicoService pontos,
        INoticiaService noticias,
        IRoteiroService roteiros,
        IGrupoCulturalService grupos,
        IPratoTuristicoService pratos,
        ITagCulturalService tags,
        IGaleriaService galeria)
    {
        _pontos = pontos;
        _noticias = noticias;
        _roteiros = roteiros;
        _grupos = grupos;
        _pratos = pratos;
        _tags = tags;
        _galeria = galeria;
    }

    /// <summary>GET /sitemap.xml — todas as rotas públicas + detalhes do banco.</summary>
    [Route("sitemap.xml")]
    [Produces("application/xml")]
    public async Task<IActionResult> Sitemap(CancellationToken ct)
    {
        // PathBase: sob sub-aplicação IIS (ex.: /turismo), o sitemap precisa
        // do caminho completo — senão as URLs apontam para a raiz do site.
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        var urls = new List<(string Loc, double Prioridade, DateTime? LastMod)>
        {
            ($"{baseUrl}/", 1.0, null),
            ($"{baseUrl}/cidade", 0.8, null),
            ($"{baseUrl}/cultura", 0.8, null),
            ($"{baseUrl}/grupos-populares", 0.8, null),
            ($"{baseUrl}/gastronomia", 0.8, null),
            ($"{baseUrl}/lugares", 0.9, null),
            ($"{baseUrl}/noticias", 0.7, null),
            ($"{baseUrl}/roteiros", 0.7, null),
            ($"{baseUrl}/galeria", 0.7, null)
        };

        // Páginas de detalhe (conteúdo do banco)
        var maravilhas = await _pontos.ListarAsync(apenasAtivos: true, ct);
        foreach (var ponto in maravilhas.Where(p => p.CategoriaApresentarEmMaravilhas))
        {
            urls.Add(($"{baseUrl}/lugares/{ponto.Id}/{Slug.De(ponto.Nome)}", 0.9, null));
        }

        var noticias = await _noticias.ListarAsync(apenasPublicadas: true, ct);
        foreach (var noticia in noticias)
        {
            urls.Add(($"{baseUrl}/noticias/{noticia.Slug}", 0.7, noticia.DataPublicacao));
        }

        var roteiros = await _roteiros.ListarAsync(ct);
        foreach (var roteiro in roteiros.Where(r => r.Ativo))
        {
            urls.Add(($"{baseUrl}/roteiros/{roteiro.Id}", 0.7, null));
        }

        var grupos = await _grupos.ListarAsync(ct);
        foreach (var grupo in grupos.Where(g => g.Ativo))
        {
            urls.Add(($"{baseUrl}/grupos-populares/{grupo.Id}", 0.6, null));
        }

        var pratos = await _pratos.ListarAsync(ct);
        foreach (var prato in pratos.Where(p => p.Ativo))
        {
            urls.Add(($"{baseUrl}/gastronomia/{prato.Id}", 0.6, null));
        }

        var tags = await _tags.ListarAsync(ct);
        foreach (var tag in tags.Where(t => t.Ativo))
        {
            urls.Add(($"{baseUrl}/cultura/{tag.Id}", 0.6, null));
        }

        var categoriasGaleria = await _galeria.ListarCategoriasAsync(incluirInativas: false, ct);
        foreach (var categoria in categoriasGaleria)
        {
            urls.Add(($"{baseUrl}/galeria/{categoria.Chave}", 0.6, null));
        }

        using var ms = new MemoryStream();
        await using (var writer = XmlWriter.Create(ms, new XmlWriterSettings
        {
            Encoding = System.Text.Encoding.UTF8,
            Indent = true,
            Async = true
        }))
        {
            await writer.WriteStartDocumentAsync();
            await writer.WriteStartElementAsync(null, "urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
            foreach (var (loc, prioridade, lastMod) in urls)
            {
                await writer.WriteStartElementAsync(null, "url", null);
                await writer.WriteElementStringAsync(null, "loc", null, loc);
                if (lastMod is DateTime lm)
                    await writer.WriteElementStringAsync(null, "lastmod", null, lm.ToString("yyyy-MM-dd"));
                await writer.WriteElementStringAsync(null, "priority", null, prioridade.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
                await writer.WriteEndElementAsync();
            }
            await writer.WriteEndElementAsync();
            await writer.WriteEndDocumentAsync();
        }

        return File(ms.ToArray(), "application/xml");
    }

    /// <summary>GET /robots.txt — libera o portal e aponta o sitemap.</summary>
    [Route("robots.txt")]
    public IActionResult Robots()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var corpo = $"User-agent: *\nAllow: /\n\nSitemap: {baseUrl}/sitemap.xml\n";
        return Content(corpo, "text/plain");
    }
}
