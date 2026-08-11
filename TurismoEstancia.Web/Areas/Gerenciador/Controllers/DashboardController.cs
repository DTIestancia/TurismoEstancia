using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Analytics.Interfaces;
using TurismoEstancia.Services.Avaliacao.Interfaces;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;
using TurismoEstancia.Services.Galeria.Interfaces;
using TurismoEstancia.Services.Roteiro.Interfaces;
using TurismoEstancia.Services.Turismo.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

/// <summary>
/// Dashboard do Gerenciador: painel de análises do portal (visitas, cliques,
/// fontes de tráfego, rankings, newsletter e SEO) + contadores de conteúdo.
/// </summary>
public class DashboardController : PainelController
{
    private readonly IPontoTuristicoService _pontos;
    private readonly IEventoService _eventos;
    private readonly INoticiaService _noticias;
    private readonly IRoteiroService _roteiros;
    private readonly IInscricaoNewsletterService _newsletter;
    private readonly IAvaliacaoService _avaliacoes;
    private readonly IGrupoCulturalService _grupos;
    private readonly IPratoTuristicoService _pratos;
    private readonly ICategoriaPontoTuristicoService _categorias;
    private readonly IAnalyticsService _analytics;
    private readonly IConfiguracaoSiteService _configs;
    private readonly ITagCulturalService _tags;
    private readonly IGaleriaService _galeria;

    public DashboardController(
        IServiceProvider services,
        IPontoTuristicoService pontos,
        IEventoService eventos,
        INoticiaService noticias,
        IRoteiroService roteiros,
        IInscricaoNewsletterService newsletter,
        IAvaliacaoService avaliacoes,
        IGrupoCulturalService grupos,
        IPratoTuristicoService pratos,
        ICategoriaPontoTuristicoService categorias,
        IAnalyticsService analytics,
        IConfiguracaoSiteService configs,
        ITagCulturalService tags,
        IGaleriaService galeria)
        : base(services)
    {
        _pontos = pontos;
        _eventos = eventos;
        _noticias = noticias;
        _roteiros = roteiros;
        _newsletter = newsletter;
        _avaliacoes = avaliacoes;
        _grupos = grupos;
        _pratos = pratos;
        _categorias = categorias;
        _analytics = analytics;
        _configs = configs;
        _tags = tags;
        _galeria = galeria;
    }

    public async Task<IActionResult> Index(int dias, int? galeriaCategoria, CancellationToken ct)
    {
        ViewData["Title"] = "Dashboard";
        if (dias is not (7 or 30 or 90)) dias = 30;

        var de = DateTime.Today.AddDays(-(dias - 1));
        var ate = DateTime.Today;

        var resumo = await _analytics.ObterResumoAsync(de, ate, galeriaCategoria, ct);

        // Categorias da galeria para o filtro do ranking de fotos (inclui inativas,
        // para o ranking de uma categoria desativada continuar consultável).
        var galeriaCategorias = await _galeria.ListarCategoriasAsync(incluirInativas: true, ct);

        var inscricoes = await _newsletter.ListarAsync(incluirInativos: true, ct);
        var novasNoPeriodo = inscricoes.Count(i => i.DataInscricao.Date >= de.Date && i.DataInscricao.Date <= ate.Date);
        var ativas = inscricoes.Count(i => i.Ativo);

        var configs = await _configs.ListarAsync(ct);
        var seoTitulo = configs.FirstOrDefault(c => c.Chave == "site-titulo")?.ValorTexto;
        var seoDescricao = configs.FirstOrDefault(c => c.Chave == "meta-descricao")?.ValorTexto;

        var itens = new List<PainelStatViewModel>
        {
            new() { Rotulo = "Pontos turísticos", Icone = "map-pin", Valor = (await _pontos.ListarAsync(apenasAtivos: false, ct)).Count },
            new() { Rotulo = "Categorias", Icone = "folder-tree", Valor = (await _categorias.ListarAsync(incluirInativos: true, ct)).Count },
            new() { Rotulo = "Eventos", Icone = "calendar", Valor = (await _eventos.ListarAsync(apenasProximos: false, ct)).Count },
            new() { Rotulo = "Notícias", Icone = "newspaper", Valor = (await _noticias.ListarAsync(apenasPublicadas: false, ct)).Count },
            new() { Rotulo = "Roteiros", Icone = "route", Valor = (await _roteiros.ListarAsync(ct)).Count },
            new() { Rotulo = "Grupos culturais", Icone = "music", Valor = (await _grupos.ListarAsync(ct)).Count },
            new() { Rotulo = "Pratos turísticos", Icone = "utensils", Valor = (await _pratos.ListarAsync(ct)).Count },
            new() { Rotulo = "Inscrições newsletter", Icone = "mail", Valor = ativas },
            new() { Rotulo = "Avaliações", Icone = "star", Valor = (await _avaliacoes.ListarAsync(apenasAprovadas: false, ct)).Count }
        };

        // Contagem real de rotas públicas no sitemap (8 estáticas + detalhes do banco).
        var maravilhas = (await _pontos.ListarAsync(apenasAtivos: true, ct)).Count(p => p.CategoriaApresentarEmMaravilhas);
        var noticiasPublicadas = (await _noticias.ListarAsync(apenasPublicadas: true, ct)).Count;
        var roteirosAtivos = (await _roteiros.ListarAsync(ct)).Count(r => r.Ativo);
        var gruposAtivos = (await _grupos.ListarAsync(ct)).Count(g => g.Ativo);
        var pratosAtivos = (await _pratos.ListarAsync(ct)).Count(p => p.Ativo);
        var tagsAtivas = (await _tags.ListarAsync(ct)).Count(t => t.Ativo);
        var rotasIndexaveis = 8 + maravilhas + noticiasPublicadas + roteirosAtivos + gruposAtivos + pratosAtivos + tagsAtivas;

        var vm = new DashboardAnalyticsViewModel
        {
            PeriodoDias = dias,
            De = de,
            Ate = ate,
            Resumo = resumo,
            NewsletterNoPeriodo = novasNoPeriodo,
            NewsletterAtivas = ativas,
            Conteudos = itens,
            RotasIndexaveis = rotasIndexaveis,
            SeoTitulo = seoTitulo,
            SeoDescricao = seoDescricao,
            GaleriaCategorias = galeriaCategorias,
            GaleriaCategoriaId = galeriaCategoria
        };

        return View(vm);
    }
}
