using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using CategoriaConhecaEstancia = TurismoEstancia.Domain.Models.CategoriaConhecaEstancia;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.ConhecaEstancia.Interfaces;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;
using TurismoEstancia.Services.Roteiro.Interfaces;
using TurismoEstancia.Services.Turismo.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Controllers;

/// <summary>Portal público — a página inicial compõe todas as seções dinâmicas.</summary>
public class HomeController : Controller
{
    private readonly ISlideService _slides;
    private readonly IEstatisticaService _estatisticas;
    private readonly IGrupoCulturalService _grupos;
    private readonly IPratoTuristicoService _pratos;
    private readonly ITagCulturalService _tags;
    private readonly IPontoTuristicoService _pontos;
    private readonly ICategoriaPontoTuristicoService _categorias;
    private readonly IEventoService _eventos;
    private readonly IRoteiroService _roteiros;
    private readonly INoticiaService _noticias;
    private readonly IConteudoSiteService _conteudos;
    private readonly IConfiguracaoSiteService _configuracoes;
    private readonly IContatoService _contatos;
    private readonly IConhecaEstanciaService _conheca;

    public HomeController(
        ISlideService slides,
        IEstatisticaService estatisticas,
        IGrupoCulturalService grupos,
        IPratoTuristicoService pratos,
        ITagCulturalService tags,
        IPontoTuristicoService pontos,
        ICategoriaPontoTuristicoService categorias,
        IEventoService eventos,
        IRoteiroService roteiros,
        INoticiaService noticias,
        IConteudoSiteService conteudos,
        IConfiguracaoSiteService configuracoes,
        IContatoService contatos,
        IConhecaEstanciaService conheca)
    {
        _slides = slides;
        _estatisticas = estatisticas;
        _grupos = grupos;
        _pratos = pratos;
        _tags = tags;
        _pontos = pontos;
        _categorias = categorias;
        _eventos = eventos;
        _roteiros = roteiros;
        _noticias = noticias;
        _conteudos = conteudos;
        _configuracoes = configuracoes;
        _contatos = contatos;
        _conheca = conheca;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var eventosProximos = await _eventos.ListarAsync(apenasProximos: true, ct);

        var vm = new HomeViewModel
        {
            Conteudos = await _conteudos.ObterDicionarioAsync(ct),
            Guia = await _configuracoes.ObterPorChaveAsync("guia-pdf", ct),
            VideoInstitucional = await _configuracoes.ObterPorChaveAsync("video-institucional", ct),
            TituloSite = await _configuracoes.ObterPorChaveAsync("site-titulo", ct),
            Slides = await _slides.ListarAsync(ct),
            Estatisticas = await _estatisticas.ListarAsync(ct),
            GruposCulturais = await _grupos.ListarAsync(ct),
            PratosTuristicos = await _pratos.ListarAsync(ct),
            TagsCulturais = await _tags.ListarAsync(ct),
            EventosProximosTotal = eventosProximos.Count,
            EventosProximos = eventosProximos.Take(3).ToList(),
            Roteiros = await _roteiros.ListarAsync(ct),
            Noticias = (await _noticias.ListarAsync(apenasPublicadas: true, ct)).Take(3).ToList(),
            Contatos = await _contatos.ListarAsync(null, ct)
        };

        var categorias = await _categorias.ListarAsync(false, ct);
        var pontos = await _pontos.ListarAsync(true, ct);

        // Maravilhas: apenas categorias marcadas para exibir na seção.
        vm.CategoriasMaravilhas = categorias
            .Where(c => c.ApresentarEmMaravilhas && c.Ativo)
            .OrderBy(c => c.Ordem)
            .Select(c => new CategoriaMaravilhasViewModel
            {
                Categoria = c,
                Pontos = pontos
                    .Where(p => p.CategoriaId == c.Id)
                    .OrderBy(p => p.Ordem)
                    .ToList()
            })
            .Where(g => g.Pontos.Count > 0)
            .ToList();

        // Mapa: todas as categorias exibíveis + pontos para o mapa.
        vm.CategoriasMapa = categorias
            .Where(c => c.ExibirNoMapa && c.Ativo)
            .OrderBy(c => c.Ordem)
            .ToList();

        vm.PontosParaMapa = pontos
            .Where(p => p.ExibirNoMapa && p.Ativo)
            .ToList();

        vm.ConhecaEstancia = await MontarConhecaEstancia(ct);

        vm.MapaJson = SerializarMapa(categorias, pontos);

        return View(vm);
    }

    /// <summary>
    /// Monta o JSON consumido pelo portal.js (formato do allPois do protótipo):
    /// categorias com chave/label/cor/ícone + pontos com posição e metadados.
    /// </summary>
    private string SerializarMapa(
        IReadOnlyList<CategoriaPontoTuristicoDto> categorias,
        IReadOnlyList<PontoTuristicoDto> pontos)
    {
        var categoriasMapa = categorias
            .Where(c => c.ExibirNoMapa && c.Ativo)
            .OrderBy(c => c.Ordem)
            .Select(c => new
            {
                key = c.Chave,
                label = c.Nome,
                color = c.Cor ?? "#F97E31",
                icon = c.Icone ?? "map-pin"
            })
            .ToList();

        var numeroMaravilha = 0;
        var pois = new List<object>();
        foreach (var p in pontos.Where(x => x.ExibirNoMapa && x.Ativo).OrderBy(x => x.Ordem))
        {
            var cat = categorias.FirstOrDefault(c => c.Id == p.CategoriaId);
            if (cat is null) continue;

            var eMaravilha = cat.ApresentarEmMaravilhas;
            if (eMaravilha) numeroMaravilha++;

            pois.Add(new
            {
                id = p.Id,
                category = cat.Chave,
                left = p.LeftPercent,
                top = p.TopPercent,
                delay = 0.05 + (numeroMaravilha - 1) * 0.05,
                content = eMaravilha ? numeroMaravilha.ToString() : LetraPoi(cat.Chave),
                title = p.Nome,
                desc = p.Descricao,
                detail = p.Detalhe,
                img = p.CapaArquivoId is long capaId ? Url.Content($"~/arquivo/{capaId}") : "",
                icon = p.Icone ?? "map-pin",
                tag = p.Tag,
                address = p.Endereco,
                directions = p.ComoChegar,
                poi = !eMaravilha
            });
        }

        return JsonSerializer.Serialize(new { categorias = categoriasMapa, pontos = pois });
    }

    private static string LetraPoi(string chave) => chave.ToLowerInvariant() switch
    {
        "hotel" => "H",
        "food" => "R",
        "service" => "S",
        _ => "•"
    };

    /// <summary>
    /// Monta a seção "Conheça Estância": quatro abas (História, Cultura,
    /// Gastronomia, Experiências) alimentadas exclusivamente pelos itens
    /// cadastrados no Gerenciador (ConhecaEstancia › Itens).
    /// </summary>
    private async Task<IReadOnlyList<ConhecaEstanciaTab>> MontarConhecaEstancia(CancellationToken ct)
    {
        var itens = await _conheca.ListarAtivosAsync(ct);

        ConhecaEstanciaTab Aba(CategoriaConhecaEstancia categoria, string rotulo, string icone) => new()
        {
            Chave = categoria.ToString().ToLowerInvariant(),
            Rotulo = rotulo,
            Icone = icone,
            Itens = itens
                .Where(i => i.Categoria == categoria)
                .Select(i => new ConhecaEstanciaItem
                {
                    Id = i.Id,
                    Nome = i.Nome,
                    Descricao = i.Descricao,
                    ImagemArquivoId = i.ImagemArquivoId,
                    ImagemZoom = i.ImagemZoom,
                    ImagemPosicaoX = i.ImagemPosicaoX,
                    ImagemPosicaoY = i.ImagemPosicaoY,
                    Url = Url.Content($"~/conheca-estancia/{i.Id}/{Slug.De(i.Nome)}")
                })
                .ToList()
        };

        return
        [
            Aba(CategoriaConhecaEstancia.Historia, "História", "landmark"),
            Aba(CategoriaConhecaEstancia.Cultura, "Cultura", "music"),
            Aba(CategoriaConhecaEstancia.Gastronomia, "Gastronomia", "utensils"),
            Aba(CategoriaConhecaEstancia.Experiencias, "Experiências", "sun")
        ];
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Páginas de erro não devem ser indexadas.
        ViewData["Seo"] = new SeoMeta { NoIndex = true };
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
