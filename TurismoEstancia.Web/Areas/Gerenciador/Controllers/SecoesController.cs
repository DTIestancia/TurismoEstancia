using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;
using TurismoEstancia.Services.Infra.Interfaces;
using TurismoEstancia.Services.Roteiro.Interfaces;
using TurismoEstancia.Services.Turismo.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

/// <summary>
/// Edição por área do portal: cada seção da home (Hero, Nossa Cidade, Cultura,
/// Gastronomia, 7 Maravilhas, Agenda, Notícias, Roteiros, Mapa e Rodapé) vira
/// uma página com os textos E imagens daquela área num formulário só — em vez
/// de "Textos do portal" genérico + "Slides do hero" separado + telas soltas.
/// Os CRUDs de lista (grupos, pratos, eventos...) continuam nas telas próprias,
/// acessíveis por atalho dentro da área.
/// </summary>
public class SecoesController : PainelController
{
    private readonly IConteudoSiteService _conteudos;
    private readonly IArquivoService _arquivos;
    private readonly ISlideService _slides;
    private readonly IEstatisticaService _estatisticas;
    private readonly IPontoTuristicoService _pontos;
    private readonly IEventoService _eventos;
    private readonly INoticiaService _noticias;
    private readonly IRoteiroService _roteiros;
    private readonly IGrupoCulturalService _grupos;
    private readonly IPratoTuristicoService _pratos;
    private readonly IContatoService _contatos;

    public SecoesController(
        IServiceProvider services,
        IConteudoSiteService conteudos,
        IArquivoService arquivos,
        ISlideService slides,
        IEstatisticaService estatisticas,
        IPontoTuristicoService pontos,
        IEventoService eventos,
        INoticiaService noticias,
        IRoteiroService roteiros,
        IGrupoCulturalService grupos,
        IPratoTuristicoService pratos,
        IContatoService contatos)
        : base(services)
    {
        _conteudos = conteudos;
        _arquivos = arquivos;
        _slides = slides;
        _estatisticas = estatisticas;
        _pontos = pontos;
        _eventos = eventos;
        _noticias = noticias;
        _roteiros = roteiros;
        _grupos = grupos;
        _pratos = pratos;
        _contatos = contatos;
    }

    // ===== Hero =====
    public async Task<IActionResult> Hero(CancellationToken ct)
    {
        ViewData["Title"] = "Hero";
        var vm = await MontarAsync("hero", "Hero", "Abertura do portal: título, chamada, imagens do carrossel, guia e vídeo.", ct,
            textos: [T("hero-titulo", "Título", "Ex.: Explore as Cores, a História e a Tradição de Estância"), T("hero-subtitulo", "Subtítulo", "Frase curta sob o título; quebras de linha viram <br>")],
            imagens: [],
            ancora: "#section-cidade");
        vm.Slides = await _slides.ListarAsync(ct);
        return View("Editar", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarHero(AreaSiteViewModel vm, CancellationToken ct)
    {
        await SalvarAsync(vm, ct);
        TempData["PainelOk"] = "Hero atualizado.";
        return RedirectToAction(nameof(Hero));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdicionarSlide(SlideDto dto, IFormFile imagem, CancellationToken ct)
    {
        if (imagem is null || imagem.Length == 0)
        {
            TempData["PainelErro"] = "Selecione uma imagem para o slide.";
            return RedirectToAction(nameof(Hero));
        }

        dto.Ativo = true;
        await _slides.SalvarAsync(dto, imagem, ct);
        TempData["PainelOk"] = "Slide adicionado ao hero.";
        return RedirectToAction(nameof(Hero));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirSlide(int id, CancellationToken ct)
    {
        await _slides.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Slide removido do hero.";
        return RedirectToAction(nameof(Hero));
    }

    // ===== Nossa Cidade =====
    public async Task<IActionResult> Cidade(CancellationToken ct)
    {
        ViewData["Title"] = "Nossa Cidade";
        var vm = await MontarAsync("cidade", "Nossa Cidade", "História da cidade, descrição, citação e a foto exibida no card.", ct,
            textos: [
                T("historia-texto", "Texto da história", "Parágrafo principal; aceita <strong> e <br>"),
                T("historia-citacao", "Citação", "Frase de efeito entre aspas"),
                T("historia-descricao", "Descrição", "Segundo parágrafo (lado direito); aceita <strong>")
            ],
            imagens: [I("historia-imagem", "Foto da cidade")],
            ancora: "#section-historia");
        vm.Estatisticas = await _estatisticas.ListarAsync(ct);
        vm.Links = [Link("Estatísticas da seção", "Números exibidos no card (anos, habitantes, maravilhas).", Url.Action("Index", "Estatisticas", new { area = "Gerenciador" }) ?? "/Gerenciador/Estatisticas", "bar-chart-3")];
        return View("Editar", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarCidade(AreaSiteViewModel vm, CancellationToken ct)
    {
        await SalvarAsync(vm, ct);
        TempData["PainelOk"] = "Nossa Cidade atualizada.";
        return RedirectToAction(nameof(Cidade));
    }

    // ===== Cultura =====
    public async Task<IActionResult> Cultura(CancellationToken ct)
    {
        ViewData["Title"] = "Cultura";
        var vm = await MontarAsync("cultura", "Cultura", "Textos da seção de cultura da home e da página de cultura.", ct,
            textos: [
                T("cultura-titulo", "Título", "Headline decorativa (aceita HTML); ex.: <div class=\"headline-white\">Nossa</div><div class=\"headline-yellow\">cultura</div><div class=\"cursive-small\">arde alto.</div>"),
                T("cultura-descricao", "Descrição", "Texto principal da seção"),
                T("cultura-extra", "Texto extra", "Complemento exibido abaixo"),
                T("cultura-citacao", "Citação", "Frase de efeito")
            ],
            imagens: [I("cultura-imagem", "Foto do card")],
            ancora: "#section-cultura");
        var grupos = await _grupos.ListarAsync(ct);
        vm.Itens = new ItensAreaViewModel
        {
            Titulo = "Grupos culturais cadastrados",
            RotuloBotao = "Cadastrar novo grupo",
            UrlCriar = Url.Action("Criar", "GruposCulturais", new { area = "Gerenciador" }) ?? "/Gerenciador/GruposCulturais/Criar",
            UrlLista = Url.Action("Index", "GruposCulturais", new { area = "Gerenciador" }) ?? "/Gerenciador/GruposCulturais",
            Icone = "music",
            Itens = grupos.Select(g => new ItemAreaViewModel { Id = g.Id, Nome = g.Nome, Detalhe = g.Descricao, ImagemArquivoId = g.ImagemArquivoId, Ativo = g.Ativo }).ToList()
        };
        return View("Editar", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarCultura(AreaSiteViewModel vm, CancellationToken ct)
    {
        await SalvarAsync(vm, ct);
        TempData["PainelOk"] = "Cultura atualizada.";
        return RedirectToAction(nameof(Cultura));
    }

    // ===== Gastronomia =====
    public async Task<IActionResult> Gastronomia(CancellationToken ct)
    {
        ViewData["Title"] = "Gastronomia";
        var vm = await MontarAsync("gastronomia", "Gastronomia e Grupos Populares", "Textos da gastronomia e dos grupos populares.", ct,
            textos: [
                T("gastronomia-titulo", "Título", "Cole o exemplo: <div class=\"gastronomy-cursive\">na mesa &amp; na rua</div><div class=\"gastronomy-title-wrap\"><div class=\"gastronomy-title-line serif-italic\">Grupos</div><div class=\"gastronomy-title-line georgia-bold\">populares</div><div class=\"gastronomy-title-ampersand\">&amp;</div><div class=\"gastronomy-title-line georgia-bold white\">gastronomia.</div></div>"),
                T("gastronomia-descricao", "Descrição", "Texto principal da gastronomia"),
                T("gastronomia-main-texto", "Texto do card principal", "O prato em destaque"),
                T("gastronomia-grupos-texto", "Texto dos grupos populares", "Usado na home e na página Grupos Populares")
            ],
            imagens: [I("gastronomia-imagem", "Foto do card principal")],
            ancora: "#section-gastronomia");
        var pratos = await _pratos.ListarAsync(ct);
        vm.Itens = new ItensAreaViewModel
        {
            Titulo = "Pratos turísticos cadastrados",
            RotuloBotao = "Cadastrar novo prato",
            UrlCriar = Url.Action("Criar", "PratosTuristicos", new { area = "Gerenciador" }) ?? "/Gerenciador/PratosTuristicos/Criar",
            UrlLista = Url.Action("Index", "PratosTuristicos", new { area = "Gerenciador" }) ?? "/Gerenciador/PratosTuristicos",
            Icone = "utensils",
            Itens = pratos.Select(p => new ItemAreaViewModel { Id = p.Id, Nome = p.Nome, Detalhe = p.Descricao, ImagemArquivoId = p.ImagemArquivoId, Ativo = p.Ativo }).ToList()
        };
        return View("Editar", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarGastronomia(AreaSiteViewModel vm, CancellationToken ct)
    {
        await SalvarAsync(vm, ct);
        TempData["PainelOk"] = "Gastronomia atualizada.";
        return RedirectToAction(nameof(Gastronomia));
    }

    // ===== 7 Maravilhas =====
    public async Task<IActionResult> Maravilhas(CancellationToken ct)
    {
        ViewData["Title"] = "7 Maravilhas";
        var vm = await MontarAsync("maravilhas", "7 Maravilhas", "Chamada do baralho de cartas e acesso às categorias e pontos.", ct,
            textos: [
                T("maravilhas-titulo", "Título", "Aceita <strong> e <span class=\"vitrine-accent\"> para destacar; ex.: Lugares que <strong>encantam</strong>"),
                T("maravilhas-descricao", "Descrição", "Chamada acima do deck")
            ],
            imagens: [],
            ancora: "#section-maravilhas");
        var pontos = await _pontos.ListarAsync(apenasAtivos: false, ct);
        // Só as 7 Maravilhas: categorias marcadas com ApresentarEmMaravilhas.
        var maravilhas = pontos.Where(p => p.CategoriaApresentarEmMaravilhas).ToList();
        vm.Itens = new ItensAreaViewModel
        {
            Titulo = "7 Maravilhas cadastradas",
            RotuloBotao = "Cadastrar nova maravilha",
            UrlCriar = Url.Action("Criar", "PontosTuristicos", new { area = "Gerenciador" }) ?? "/Gerenciador/PontosTuristicos/Criar",
            UrlLista = Url.Action("Index", "PontosTuristicos", new { area = "Gerenciador", contexto = "maravilhas" }) ?? "/Gerenciador/PontosTuristicos?contexto=maravilhas",
            Icone = "gem",
            Itens = maravilhas.Select(p => new ItemAreaViewModel { Id = p.Id, Nome = p.Nome, Detalhe = p.CategoriaNome, ImagemArquivoId = p.CapaArquivoId, Ativo = p.Ativo }).ToList()
        };
        return View("Editar", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarMaravilhas(AreaSiteViewModel vm, CancellationToken ct)
    {
        await SalvarAsync(vm, ct);
        TempData["PainelOk"] = "7 Maravilhas atualizada.";
        return RedirectToAction(nameof(Maravilhas));
    }

    // ===== Agenda =====
    public async Task<IActionResult> Agenda(CancellationToken ct)
    {
        ViewData["Title"] = "Agenda";
        var vm = await MontarAsync("agenda", "Agenda", "Programação oficial de eventos.", ct,
            textos: [
                T("agenda-titulo", "Título", "Aceita <br> para quebrar linha"),
                T("agenda-subtitulo", "Subtítulo", "Frase sob o título"),
                T("agenda-pill", "Selo da agenda", "Ex.: Programação Oficial")
            ],
            imagens: [],
            ancora: "#section-servicos");
        var eventos = await _eventos.ListarAsync(apenasProximos: false, ct);
        vm.Itens = new ItensAreaViewModel
        {
            Titulo = "Eventos cadastrados",
            RotuloBotao = "Cadastrar novo evento",
            UrlCriar = Url.Action("Criar", "Eventos", new { area = "Gerenciador" }) ?? "/Gerenciador/Eventos/Criar",
            UrlLista = Url.Action("Index", "Eventos", new { area = "Gerenciador" }) ?? "/Gerenciador/Eventos",
            Icone = "calendar",
            Itens = eventos.Select(e => new ItemAreaViewModel { Id = e.Id, Nome = e.Titulo, Detalhe = e.Local, Ativo = e.Ativo }).ToList()
        };
        return View("Editar", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarAgenda(AreaSiteViewModel vm, CancellationToken ct)
    {
        await SalvarAsync(vm, ct);
        TempData["PainelOk"] = "Agenda atualizada.";
        return RedirectToAction(nameof(Agenda));
    }

    // ===== Notícias =====
    public async Task<IActionResult> Noticias(CancellationToken ct)
    {
        ViewData["Title"] = "Notícias";
        var vm = await MontarAsync("noticias", "Notícias", "Chamada da seção e acesso às notícias publicadas.", ct,
            textos: [
                T("noticias-titulo", "Título", "Aceita <span class=\"secao-destaque\"> para destacar; ex.: Notícias de <span class=\"secao-destaque\">Estância</span>"),
                T("noticias-descricao", "Descrição", "Chamada da seção de notícias")
            ],
            imagens: [],
            ancora: "#section-noticias");
        var noticias = await _noticias.ListarAsync(apenasPublicadas: false, ct);
        vm.Itens = new ItensAreaViewModel
        {
            Titulo = "Notícias cadastradas",
            RotuloBotao = "Cadastrar nova notícia",
            UrlCriar = Url.Action("Criar", "Noticias", new { area = "Gerenciador" }) ?? "/Gerenciador/Noticias/Criar",
            UrlLista = Url.Action("Index", "Noticias", new { area = "Gerenciador" }) ?? "/Gerenciador/Noticias",
            Icone = "newspaper",
            Itens = noticias.Select(n => new ItemAreaViewModel { Id = n.Id, Nome = n.Titulo, Detalhe = n.DataPublicacao.ToString("dd/MM/yyyy"), ImagemArquivoId = n.ImagemArquivoId, Ativo = n.Ativo }).ToList()
        };
        return View("Editar", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarNoticias(AreaSiteViewModel vm, CancellationToken ct)
    {
        await SalvarAsync(vm, ct);
        TempData["PainelOk"] = "Notícias atualizada.";
        return RedirectToAction(nameof(Noticias));
    }

    // ===== Roteiros =====
    public async Task<IActionResult> Roteiros(CancellationToken ct)
    {
        ViewData["Title"] = "Roteiros";
        var vm = await MontarAsync("roteiros", "Roteiros", "Chamada da seção e acesso aos roteiros.", ct,
            textos: [
                T("roteiros-titulo", "Título", "Aceita <span class=\"secao-destaque\"> para destacar; ex.: Trilhas para <span class=\"secao-destaque\">explorar</span>"),
                T("roteiros-descricao", "Descrição", "Chamada da seção de roteiros")
            ],
            imagens: [],
            ancora: "#section-roteiros");
        var roteiros = await _roteiros.ListarAsync(ct);
        vm.Itens = new ItensAreaViewModel
        {
            Titulo = "Roteiros cadastrados",
            RotuloBotao = "Cadastrar novo roteiro",
            UrlCriar = Url.Action("Criar", "Roteiros", new { area = "Gerenciador" }) ?? "/Gerenciador/Roteiros/Criar",
            UrlLista = Url.Action("Index", "Roteiros", new { area = "Gerenciador" }) ?? "/Gerenciador/Roteiros",
            Icone = "route",
            Itens = roteiros.Select(r => new ItemAreaViewModel { Id = r.Id, Nome = r.Titulo, Detalhe = r.Descricao, ImagemArquivoId = r.ImagemArquivoId, Ativo = r.Ativo }).ToList()
        };
        return View("Editar", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarRoteiros(AreaSiteViewModel vm, CancellationToken ct)
    {
        await SalvarAsync(vm, ct);
        TempData["PainelOk"] = "Roteiros atualizada.";
        return RedirectToAction(nameof(Roteiros));
    }

    // ===== Mapa =====
    public async Task<IActionResult> Mapa(CancellationToken ct)
    {
        ViewData["Title"] = "Mapa";
        var vm = await MontarAsync("mapa", "Mapa interativo", "Chamada do mapa e acesso às categorias e pontos.", ct,
            textos: [
                T("mapa-titulo", "Título", "Aceita <span class=\"secao-destaque\"> para destacar; ex.: Encontre as <span class=\"secao-destaque\">7 Maravilhas</span>"),
                T("mapa-descricao", "Descrição", "Chamada do mapa interativo")
            ],
            imagens: [],
            ancora: "#section-mapa");
        var pontosMapa = await _pontos.ListarAsync(apenasAtivos: false, ct);
        // Mapa: todos os pontos com ExibirNoMapa — maravilhas, restaurantes,
        // hotéis, praias e demais pontos turísticos.
        var mapa = pontosMapa.Where(p => p.ExibirNoMapa).ToList();
        vm.Itens = new ItensAreaViewModel
        {
            Titulo = "Pontos no mapa",
            RotuloBotao = "Cadastrar novo ponto",
            UrlCriar = Url.Action("Criar", "PontosTuristicos", new { area = "Gerenciador" }) ?? "/Gerenciador/PontosTuristicos/Criar",
            UrlLista = Url.Action("Index", "PontosTuristicos", new { area = "Gerenciador", contexto = "mapa" }) ?? "/Gerenciador/PontosTuristicos?contexto=mapa",
            Icone = "map",
            Itens = mapa.Select(p => new ItemAreaViewModel { Id = p.Id, Nome = p.Nome, Detalhe = p.CategoriaNome, ImagemArquivoId = p.CapaArquivoId, Ativo = p.Ativo }).ToList()
        };
        return View("Editar", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarMapa(AreaSiteViewModel vm, CancellationToken ct)
    {
        await SalvarAsync(vm, ct);
        TempData["PainelOk"] = "Mapa atualizado.";
        return RedirectToAction(nameof(Mapa));
    }

    // ===== Rodapé =====
    public async Task<IActionResult> Rodape(CancellationToken ct)
    {
        ViewData["Title"] = "Rodapé";
        var vm = await MontarAsync("rodape", "Rodapé", "Chamada da newsletter e acesso aos contatos exibidos no rodapé.", ct,
            textos: [T("newsletter-titulo", "Título da newsletter", "Chamada acima do campo de e-mail")],
            imagens: [],
            ancora: "#site-footer");
        var contatos = await _contatos.ListarAsync(null, ct);
        vm.Itens = new ItensAreaViewModel
        {
            Titulo = "Contatos do rodapé cadastrados",
            RotuloBotao = "Cadastrar novo contato",
            UrlCriar = Url.Action("Criar", "Contatos", new { area = "Gerenciador" }) ?? "/Gerenciador/Contatos/Criar",
            UrlLista = Url.Action("Index", "Contatos", new { area = "Gerenciador" }) ?? "/Gerenciador/Contatos",
            Icone = "phone",
            Itens = contatos.Select(c => new ItemAreaViewModel
            {
                Id = c.Id,
                Nome = c.Rotulo ?? c.Tipo.ToString(),
                Detalhe = c.Valor,
                Ativo = c.Ativo
            }).ToList()
        };
        return View("Editar", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarRodape(AreaSiteViewModel vm, CancellationToken ct)
    {
        await SalvarAsync(vm, ct);
        TempData["PainelOk"] = "Rodapé atualizado.";
        return RedirectToAction(nameof(Rodape));
    }

    // ===== Helpers =====
    private async Task<AreaSiteViewModel> MontarAsync(
        string area, string titulo, string descricao, CancellationToken ct,
        IReadOnlyList<CampoTextoArea> textos, IReadOnlyList<CampoImagemArea> imagens,
        string? ancora = null)
    {
        var dicionario = await _conteudos.ObterDicionarioAsync(ct);

        var vm = new AreaSiteViewModel
        {
            Area = area,
            Titulo = titulo,
            Descricao = descricao,
            VerSecaoUrl = ancora is null ? null : Url.Content("~/" + ancora.TrimStart('/'))
        };

        foreach (var t in textos)
            vm.Textos.Add(new CampoTextoArea
            {
                Chave = t.Chave,
                Nome = t.Nome,
                Dica = t.Dica,
                AceitaHtml = t.AceitaHtml,
                Valor = dicionario.GetValueOrDefault(t.Chave)
            });

        foreach (var i in imagens)
        {
            var valor = dicionario.GetValueOrDefault(i.Chave);
            vm.Imagens.Add(new CampoImagemArea
            {
                Chave = i.Chave,
                Nome = i.Nome,
                Valor = valor,
                ArquivoId = long.TryParse(valor, out var id) ? id : null
            });
        }

        return vm;
    }

    private async Task SalvarAsync(AreaSiteViewModel vm, CancellationToken ct)
    {
        foreach (var t in vm.Textos)
            await _conteudos.SalvarPorChaveAsync(t.Chave, t.Nome, t.Valor, ct);

        foreach (var i in vm.Imagens)
        {
            // Upload novo para o campo (input name="imagem_{chave}").
            var arquivo = Request.Form.Files.GetFile("imagem_" + i.Chave);
            if (arquivo is { Length: > 0 })
            {
                var antigoId = long.TryParse(i.Valor, out var antigo) && antigo > 0
                    ? antigo
                    : (long?)null;

                var id = await _arquivos.SalvarAsync(arquivo, ct);
                await _conteudos.SalvarPorChaveAsync(i.Chave, i.Nome, id.ToString(), ct);

                // Remove a imagem antiga só após o commit do novo valor.
                if (antigoId.HasValue)
                    await _arquivos.ExcluirAsync(antigoId.Value, ct);
            }
        }
    }

    private static CampoTextoArea T(string chave, string nome, string? dica = null, bool aceitaHtml = true) =>
        new() { Chave = chave, Nome = nome, Dica = dica, AceitaHtml = aceitaHtml };

    private static CampoImagemArea I(string chave, string nome) => new() { Chave = chave, Nome = nome };

    private static LinkArea Link(string titulo, string descricao, string url, string icone = "arrow-right") =>
        new() { Titulo = titulo, Descricao = descricao, Url = url, Icone = icone };
}
