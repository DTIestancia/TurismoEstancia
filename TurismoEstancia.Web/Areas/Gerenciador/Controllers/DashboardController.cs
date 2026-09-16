using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Analytics.Interfaces;
using TurismoEstancia.Services.Avaliacao.Interfaces;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;
using TurismoEstancia.Services.Galeria.Interfaces;
using TurismoEstancia.Services.Planeje.Interfaces;
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
    private readonly IPlanejeService _planeje;

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
        IGaleriaService galeria,
        IPlanejeService planeje)
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
        _planeje = planeje;
    }

    public async Task<IActionResult> Index(int dias, DateTime? de, DateTime? ate, int? galeriaCategoria, CancellationToken ct)
    {
        ViewData["Title"] = "Dashboard";

        // Presets (hoje, 7, 30, 90 dias) ou intervalo personalizado via de/ate.
        var hoje = DateTime.Today;
        DateTime inicio, fim;
        int? preset = null;
        if (de.HasValue || ate.HasValue)
        {
            fim = (ate ?? hoje).Date;
            inicio = (de ?? fim).Date;
            if (inicio > fim)
                (inicio, fim) = (fim, inicio);
            if (fim > hoje)
                fim = hoje;
            if ((fim - inicio).TotalDays > 365)
                inicio = fim.AddDays(-365);
            var tamanhoPersonalizado = (fim - inicio).Days + 1;
            if (fim == hoje && tamanhoPersonalizado is 1 or 7 or 30 or 90)
                preset = tamanhoPersonalizado;
        }
        else
        {
            if (dias is not (1 or 7 or 30 or 90)) dias = 30;
            preset = dias;
            fim = hoje;
            inicio = fim.AddDays(-(dias - 1));
        }

        var tamanho = (fim - inicio).Days + 1;
        var resumo = await _analytics.ObterResumoAsync(inicio, fim, galeriaCategoria, ct);
        var anterior = await _analytics.ObterResumoAsync(inicio.AddDays(-tamanho), inicio.AddDays(-1), null, ct);

        // Categorias da galeria para o filtro do ranking de fotos (inclui inativas,
        // para o ranking de uma categoria desativada continuar consultável).
        var galeriaCategorias = await _galeria.ListarCategoriasAsync(incluirInativas: true, ct);

        var inscricoes = await _newsletter.ListarAsync(incluirInativos: true, ct);
        var novasNoPeriodo = inscricoes.Count(i => i.DataInscricao.Date >= inicio.Date && i.DataInscricao.Date <= fim.Date);
        var ativas = inscricoes.Count(i => i.Ativo);

        var configs = await _configs.ListarAsync(ct);
        var seoTitulo = configs.FirstOrDefault(c => c.Chave == "site-titulo")?.ValorTexto;
        var seoDescricao = configs.FirstOrDefault(c => c.Chave == "meta-descricao")?.ValorTexto;

        var pontosTodos = await _pontos.ListarAsync(apenasAtivos: false, ct);
        var categoriasTodas = await _categorias.ListarAsync(incluirInativos: true, ct);
        var eventosTodos = await _eventos.ListarAsync(apenasProximos: false, ct);
        var noticiasTodas = await _noticias.ListarAsync(apenasPublicadas: false, ct);
        var roteirosTodos = await _roteiros.ListarAsync(ct);
        var gruposTodos = await _grupos.ListarAsync(ct);
        var pratosTodos = await _pratos.ListarAsync(ct);
        var avaliacoesTodas = await _avaliacoes.ListarAsync(apenasAprovadas: false, ct);
        var planejeTodas = await _planeje.ListarTodasAvaliacoesAsync(ct);

        var itens = new List<PainelStatViewModel>
        {
            new() { Rotulo = "Pontos turísticos", Icone = "map-pin", Valor = pontosTodos.Count, Url = Url.Action("Index", "PontosTuristicos") },
            new() { Rotulo = "Categorias", Icone = "folder-tree", Valor = categoriasTodas.Count, Url = Url.Action("Index", "Categorias") },
            new() { Rotulo = "Eventos", Icone = "calendar", Valor = eventosTodos.Count, Url = Url.Action("Index", "Eventos") },
            new() { Rotulo = "Notícias", Icone = "newspaper", Valor = noticiasTodas.Count, Url = Url.Action("Index", "Noticias") },
            new() { Rotulo = "Roteiros", Icone = "route", Valor = roteirosTodos.Count, Url = Url.Action("Index", "Roteiros") },
            new() { Rotulo = "Grupos culturais", Icone = "music", Valor = gruposTodos.Count, Url = Url.Action("Index", "GruposCulturais") },
            new() { Rotulo = "Pratos turísticos", Icone = "utensils", Valor = pratosTodos.Count, Url = Url.Action("Index", "PratosTuristicos") },
            new() { Rotulo = "Inscrições newsletter", Icone = "mail", Valor = ativas, Url = Url.Action("Index", "Newsletter") },
            new() { Rotulo = "Avaliações", Icone = "star", Valor = avaliacoesTodas.Count + planejeTodas.Count, Url = Url.Action("Index", "Avaliacoes") }
        };

        // Contagem real de rotas públicas no sitemap (11 estáticas + detalhes do banco).
        var maravilhas = pontosTodos.Count(p => p.Ativo && p.CategoriaApresentarEmMaravilhas);
        var noticiasPublicadas = noticiasTodas.Count(n => n.Publicada && n.Ativo);
        var roteirosAtivos = roteirosTodos.Count(r => r.Ativo);
        var gruposAtivos = gruposTodos.Count(g => g.Ativo);
        var pratosAtivos = pratosTodos.Count(p => p.Ativo);
        var tagsAtivas = (await _tags.ListarAsync(ct)).Count(t => t.Ativo);
        var rotasIndexaveis = 11 + maravilhas + noticiasPublicadas + roteirosAtivos + gruposAtivos + pratosAtivos + tagsAtivas;

        static bool NoPeriodo(DateTime data, DateTime ini, DateTime f) => data.Date >= ini.Date && data.Date <= f.Date;

        var vm = new DashboardAnalyticsViewModel
        {
            PeriodoDias = tamanho,
            PresetDias = preset,
            De = inicio,
            Ate = fim,
            Resumo = resumo,
            VisitasAnteriores = anterior.Visitas,
            CliquesAnteriores = anterior.Cliques,
            AvaliacoesNoPeriodo = avaliacoesTodas.Count(a => NoPeriodo(a.Data, inicio, fim))
                + planejeTodas.Count(a => NoPeriodo(a.Data, inicio, fim)),
            PublicacoesNoPeriodo = noticiasTodas.Count(n => n.Publicada && NoPeriodo(n.DataPublicacao, inicio, fim))
                + eventosTodos.Count(e => NoPeriodo(e.DataInicio, inicio, fim)),
            AvaliacoesPendentes = avaliacoesTodas.Count(a => !a.Aprovada) + planejeTodas.Count(a => !a.Aprovada),
            ItensInativos = pontosTodos.Count(p => !p.Ativo) + categoriasTodas.Count(c => !c.Ativo)
                + eventosTodos.Count(e => !e.Ativo) + noticiasTodas.Count(n => !n.Ativo)
                + roteirosTodos.Count(r => !r.Ativo) + gruposTodos.Count(g => !g.Ativo)
                + pratosTodos.Count(p => !p.Ativo),
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
