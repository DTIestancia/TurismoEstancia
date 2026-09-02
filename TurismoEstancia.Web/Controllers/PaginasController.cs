using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Avaliacao.Interfaces;
using TurismoEstancia.Services.ConhecaEstancia.Interfaces;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;
using TurismoEstancia.Services.Roteiro.Interfaces;
using TurismoEstancia.Services.Turismo.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Controllers;

/// <summary>
/// Páginas internas do portal: cada seção da home ganha uma página completa
/// (cidade, cultura, grupos populares, gastronomia, lugares) e cada item tem
/// uma página de detalhe com informações ampliadas.
/// </summary>
public class PaginasController : Controller
{
    private readonly IConteudoSiteService _conteudos;
    private readonly IEstatisticaService _estatisticas;
    private readonly ISlideService _slides;
    private readonly IGrupoCulturalService _grupos;
    private readonly IPratoTuristicoService _pratos;
    private readonly ITagCulturalService _tags;
    private readonly IPontoTuristicoService _pontos;
    private readonly ICategoriaPontoTuristicoService _categorias;
    private readonly IAvaliacaoService _avaliacoes;
    private readonly IRoteiroService _roteiros;
    private readonly IConhecaEstanciaService _conheca;

    public PaginasController(
        IConteudoSiteService conteudos,
        IEstatisticaService estatisticas,
        ISlideService slides,
        IGrupoCulturalService grupos,
        IPratoTuristicoService pratos,
        ITagCulturalService tags,
        IPontoTuristicoService pontos,
        ICategoriaPontoTuristicoService categorias,
        IAvaliacaoService avaliacoes,
        IRoteiroService roteiros,
        IConhecaEstanciaService conheca)
    {
        _conteudos = conteudos;
        _estatisticas = estatisticas;
        _slides = slides;
        _grupos = grupos;
        _pratos = pratos;
        _tags = tags;
        _pontos = pontos;
        _categorias = categorias;
        _avaliacoes = avaliacoes;
        _roteiros = roteiros;
        _conheca = conheca;
    }

    /// <summary>GET /cidade — história completa da cidade.</summary>
    [Route("cidade")]
    public async Task<IActionResult> Cidade(CancellationToken ct)
    {
        DefinirSeo("Nossa Cidade",
            "História, patrimônio, praias e cultura: conheça Estância, a Capital Sergipana da Cultura.");
        var vm = new SecaoCidadeViewModel
        {
            Conteudos = await _conteudos.ObterDicionarioAsync(ct),
            Estatisticas = await _estatisticas.ListarAsync(ct),
            Slides = await _slides.ListarAsync(ct)
        };
        return View(vm);
    }

    /// <summary>GET /cultura — textos + tags culturais.</summary>
    [Route("cultura")]
    public async Task<IActionResult> Cultura(CancellationToken ct)
    {
        DefinirSeo("Nossa Cultura",
            "Filarmônicas, Barco de Fogo, São João e as tradições que fazem de Estância a Capital Sergipana da Cultura.");
        var vm = new SecaoCulturaViewModel
        {
            Conteudos = await _conteudos.ObterDicionarioAsync(ct),
            Tags = await _tags.ListarAsync(ct),
            Slides = await _slides.ListarAsync(ct)
        };
        return View(vm);
    }

    /// <summary>GET /grupos-populares — grupos culturais.</summary>
    [Route("grupos-populares")]
    public async Task<IActionResult> GruposPopulares(CancellationToken ct)
    {
        DefinirSeo("Grupos Populares",
            "Reisado, Cacumbi, Batucada e quadrilhas: os grupos populares que animam as festas de Estância.");
        var vm = new SecaoGruposViewModel
        {
            Conteudos = await _conteudos.ObterDicionarioAsync(ct),
            Grupos = await _grupos.ListarAsync(ct)
        };
        return View(vm);
    }

    /// <summary>GET /gastronomia — pratos turísticos.</summary>
    [Route("gastronomia")]
    public async Task<IActionResult> Gastronomia(CancellationToken ct)
    {
        DefinirSeo("Gastronomia",
            "Ginga com tapioca, moqueca de camarão e os sabores autênticos do litoral sergipano em Estância.");
        var vm = new SecaoGastronomiaViewModel
        {
            Conteudos = await _conteudos.ObterDicionarioAsync(ct),
            Pratos = await _pratos.ListarAsync(ct)
        };
        return View(vm);
    }

    /// <summary>GET /lugares — baralho das 7 maravilhas.</summary>
    [Route("lugares")]
    public async Task<IActionResult> Lugares(CancellationToken ct)
    {
        DefinirSeo("Lugares que Encantam",
            "As 7 maravilhas de Estância: praias, história e natureza de tirar o fôlego.");
        var categorias = await _categorias.ListarAsync(false, ct);
        var pontos = await _pontos.ListarAsync(true, ct);

        var maravilhas = pontos
            .Where(p => p.Ativo && p.CategoriaApresentarEmMaravilhas)
            .OrderBy(p => p.Ordem)
            .ToList();

        var vm = new SecaoLugaresViewModel
        {
            Conteudos = await _conteudos.ObterDicionarioAsync(ct),
            Maravilhas = maravilhas,
            Categorias = categorias
                .Where(c => c.ApresentarEmMaravilhas && c.Ativo)
                .OrderBy(c => c.Ordem)
                .ToList()
        };
        return View(vm);
    }

    /// <summary>GET /lugares/{id}/{slug?} — detalhe do ponto turístico.</summary>
    [Route("lugares/{id:int}/{slug?}")]
    public async Task<IActionResult> DetalheLugar(int id, string? slug, CancellationToken ct)
    {
        var lugar = await _pontos.ObterPorIdAsync(id, ct);
        if (lugar is null || !lugar.Ativo)
            return NotFound();

        // Slug apenas estético (SEO): se vier errado, redireciona para o correto.
        var slugCorreto = Slug.De(lugar.Nome);
        if (!string.Equals(slug, slugCorreto, StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(DetalheLugar), "Paginas", new { id, slug = slugCorreto });

        var roteiros = await _roteiros.ListarAsync(ct);

        DefinirSeo(lugar.Nome, lugar.Descricao,
            lugar.CapaArquivoId is long capaId ? Url.Content($"~/arquivo/{capaId}") : null);

        var vm = new DetalheLugarViewModel
        {
            Lugar = lugar,
            Avaliacoes = await _avaliacoes.ListarPorPontoAsync(id, apenasAprovadas: true, ct),
            RoteirosComPonto = roteiros
                .Where(r => r.Itens.Any(i => i.PontoTuristicoId == id))
                .ToList()
        };
        return View(vm);
    }

    /// <summary>GET /grupos-populares/{id}/{slug?} — detalhe do grupo cultural.</summary>
    [Route("grupos-populares/{id:int}/{slug?}")]
    public async Task<IActionResult> DetalheGrupo(int id, string? slug, CancellationToken ct)
    {
        var grupo = await _grupos.ObterPorIdAsync(id, ct);
        if (grupo is null || !grupo.Ativo)
            return NotFound();

        var slugCorreto = Slug.De(grupo.Nome);
        if (!string.Equals(slug, slugCorreto, StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(DetalheGrupo), "Paginas", new { id, slug = slugCorreto });

        DefinirSeo(grupo.Nome, grupo.Descricao,
            grupo.ImagemArquivoId is long imgId ? Url.Content($"~/arquivo/{imgId}") : null);
        return View(grupo);
    }

    /// <summary>GET /gastronomia/{id}/{slug?} — detalhe do prato.</summary>
    [Route("gastronomia/{id:int}/{slug?}")]
    public async Task<IActionResult> DetalhePrato(int id, string? slug, CancellationToken ct)
    {
        var prato = await _pratos.ObterPorIdAsync(id, ct);
        if (prato is null || !prato.Ativo)
            return NotFound();

        var slugCorreto = Slug.De(prato.Nome);
        if (!string.Equals(slug, slugCorreto, StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(DetalhePrato), "Paginas", new { id, slug = slugCorreto });

        DefinirSeo(prato.Nome, prato.Descricao,
            prato.ImagemArquivoId is long imgId ? Url.Content($"~/arquivo/{imgId}") : null);
        return View(prato);
    }

    /// <summary>GET /cultura/{id}/{slug?} — detalhe da tag cultural.</summary>
    [Route("cultura/{id:int}/{slug?}")]
    public async Task<IActionResult> DetalheTag(int id, string? slug, CancellationToken ct)
    {
        var tag = await _tags.ObterPorIdAsync(id, ct);
        if (tag is null || !tag.Ativo)
            return NotFound();

        var slugCorreto = Slug.De(tag.Nome);
        if (!string.Equals(slug, slugCorreto, StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(DetalheTag), "Paginas", new { id, slug = slugCorreto });

        DefinirSeo(tag.Nome, tag.Descricao,
            tag.ImagemArquivoId is long imgId ? Url.Content($"~/arquivo/{imgId}") : null);
        return View(tag);
    }

    /// <summary>GET /conheca-estancia/{id}/{slug?} — detalhe do Conheça Estância (estilo blog).</summary>
    [Route("conheca-estancia/{id:int}/{slug?}")]
    public async Task<IActionResult> DetalheConhecaEstancia(int id, string? slug, CancellationToken ct)
    {
        var item = await _conheca.ObterPorIdAsync(id, ct);
        if (item is null || !item.Ativo)
            return NotFound();

        var slugCorreto = Slug.De(item.Nome);
        if (!string.Equals(slug, slugCorreto, StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(DetalheConhecaEstancia), "Paginas", new { id, slug = slugCorreto });

        DefinirSeo(item.Nome, item.Descricao,
            item.ImagemArquivoId is long imgId ? Url.Content($"~/arquivo/{imgId}") : null);
        return View(item);
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
