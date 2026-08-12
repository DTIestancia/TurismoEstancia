using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Analytics.Interfaces;
using TurismoEstancia.Services.Galeria.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Controllers;

/// <summary>
/// Galeria de fotos da Estância no portal: /galeria mostra todas as categorias
/// (filtro em pílulas) e /galeria/{chave} mostra apenas uma categoria.
/// Também expõe os endpoints de engajamento (visualização e curtida "Amei").
/// </summary>
public class GaleriaController : Controller
{
    private readonly IGaleriaService _galeria;
    private readonly IAnalyticsService _analytics;

    public GaleriaController(IGaleriaService galeria, IAnalyticsService analytics)
    {
        _galeria = galeria;
        _analytics = analytics;
    }

    /// <summary>Fotos por página no grid (com paginação).</summary>
    private const int FotosPorPagina = 12;

    /// <summary>GET /galeria — todas as fotos das categorias ativas, paginadas.</summary>
    [Route("galeria")]
    public async Task<IActionResult> Index(CancellationToken ct, int pagina = 1)
    {
        var categorias = await _galeria.ListarCategoriasAsync(incluirInativas: false, ct);
        var fotos = await _galeria.ListarFotosTodasAsync(apenasAtivos: true, ct);

        DefinirSeo("Galeria de Fotos",
            "As imagens que contam a história de Estância: praias, patrimônio, cultura e tradições.",
            fotos.FirstOrDefault()?.ArquivoId is long capa ? Url.Content($"~/arquivo/{capa}") : null);

        return View(MontarViewModel(categorias, null, fotos, pagina));
    }

    /// <summary>GET /galeria/{chave} — fotos de uma categoria específica, paginadas.</summary>
    [Route("galeria/{chave}")]
    public async Task<IActionResult> Categoria(string chave, CancellationToken ct, int pagina = 1)
    {
        var categoria = await _galeria.ObterCategoriaPorChaveAsync(chave, ct);
        if (categoria is null)
            return NotFound();

        var categorias = await _galeria.ListarCategoriasAsync(incluirInativas: false, ct);

        // OG/SEO: a capa da categoria tem precedência; sem capa, usa a 1ª foto.
        var imagemSeo = categoria.CapaArquivoId is long capa
            ? Url.Content($"~/arquivo/{capa}")
            : categoria.Midias.FirstOrDefault()?.ArquivoId is long foto ? Url.Content($"~/arquivo/{foto}") : null;

        DefinirSeo(categoria.Nome,
            categoria.Descricao ?? $"Fotos da categoria {categoria.Nome} da Galeria de Estância.",
            imagemSeo);

        return View("Index", MontarViewModel(categorias, categoria, categoria.Midias, pagina));
    }

    /// <summary>Ordena por mais visualizadas e pagina o grid (preserva o total real no contador).</summary>
    private GaleriaViewModel MontarViewModel(
        IReadOnlyList<GaleriaCategoriaDto> categorias,
        GaleriaCategoriaDto? categoriaAtual,
        IReadOnlyList<GaleriaMidiaDto> fotos,
        int pagina)
    {
        var total = fotos.Count;
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)FotosPorPagina));
        var paginaAtual = Math.Clamp(pagina, 1, totalPaginas);

        return new GaleriaViewModel
        {
            Categorias = categorias,
            CategoriaAtual = categoriaAtual,
            FotosTotal = total,
            PaginaAtual = paginaAtual,
            PaginasTotal = totalPaginas,
            TamanhoPagina = FotosPorPagina,
            // Grid paginado, mais visualizadas primeiro.
            Fotos = fotos
                .OrderByDescending(f => f.Visualizacoes)
                .Skip((paginaAtual - 1) * FotosPorPagina)
                .Take(FotosPorPagina)
                .ToList()
        };
    }

    /// <summary>
    /// POST /galeria/visualizar/{id} — contabiliza uma visualização (lightbox aberto)
    /// e grava o evento de analytics em background. Retorna o novo total.
    /// </summary>
    [HttpPost("galeria/visualizar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Visualizar(int id, CancellationToken ct)
    {
        int total;
        try
        {
            total = await _galeria.RegistrarVisualizacaoAsync(id, ct);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }

        RegistrarEvento("visualizacao-foto", id, null);
        return Json(new { visualizacoes = total });
    }

    /// <summary>
    /// POST /galeria/curtir/{id} — curtida "Amei" com dedup por sessão anônima.
    /// Retorna o total atualizado e se esta sessão já tinha curtido.
    /// </summary>
    [HttpPost("galeria/curtir/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Curtir(int id, CancellationToken ct)
    {
        var sessaoId = Request.Cookies["te_sessao"];
        if (string.IsNullOrEmpty(sessaoId))
            return Json(new { curtidas = 0, jaCurtiu = false });

        GaleriaCurtidaResultado resultado;
        try
        {
            resultado = await _galeria.CurtirAsync(id, sessaoId, ct);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }

        if (resultado.Curtiu)
            RegistrarEvento("like-foto", id, resultado.Titulo);

        return Json(new { curtidas = resultado.Curtidas, jaCurtiu = resultado.JaCurtiu || resultado.Curtiu });
    }

    /// <summary>Enfileira um evento de analytics (mesmo padrão do beacon do portal).</summary>
    private void RegistrarEvento(string evento, int entidadeId, string? entidadeNome)
    {
        var sessaoId = Request.Cookies["te_sessao"];
        if (string.IsNullOrEmpty(sessaoId)) return;

        _analytics.Registrar(new AnalyticsEventoDto
        {
            Tipo = "Clique",
            Rota = Request.Path.ToString(),
            SessaoId = sessaoId,
            Dispositivo = DetectarDispositivo(Request.Headers.UserAgent.ToString()),
            Evento = evento,
            EntidadeId = entidadeId,
            EntidadeNome = entidadeNome
        });
    }

    private static string DetectarDispositivo(string? userAgent)
    {
        var ua = (userAgent ?? "").ToLowerInvariant();
        if (ua.Contains("ipad") || ua.Contains("tablet")) return "Tablet";
        if (ua.Contains("android") || ua.Contains("mobile") || ua.Contains("iphone")) return "Mobile";
        return "Desktop";
    }

    /// <summary>Preenche ViewData["Seo"] com os metadados da página atual.</summary>
    private void DefinirSeo(string titulo, string? descricao = null, string? imagem = null)
    {
        ViewData["Seo"] = new SeoMeta
        {
            Titulo = titulo,
            Descricao = descricao,
            ImagemUrl = imagem
        };
    }
}
