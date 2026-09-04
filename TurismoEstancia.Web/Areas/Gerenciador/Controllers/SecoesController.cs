using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.ConhecaEstancia.Interfaces;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Services.Infra.Interfaces;
using TurismoEstancia.Services.Roteiro.Interfaces;
using TurismoEstancia.Services.Turismo.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

/// <summary>
/// Edição por área do portal: cada seção da home (Hero, Nossa Cidade,
/// Conheça Estância, 7 Maravilhas, Agenda, Notícias, Roteiros, Mapa e Rodapé)
/// vira uma página com os textos E imagens daquela área num formulário só.
/// </summary>
public class SecoesController : PainelController
{
    private readonly IConteudoSiteService _conteudos;
    private readonly IArquivoService _arquivos;
    private readonly IEstatisticaService _estatisticas;
    private readonly IPontoTuristicoService _pontos;
    private readonly IEventoService _eventos;
    private readonly INoticiaService _noticias;
    private readonly IRoteiroService _roteiros;
    private readonly IContatoService _contatos;
    private readonly IConhecaEstanciaService _conheca;
    private readonly IConfiguracaoSiteService _configuracoes;

    public SecoesController(
        IServiceProvider services,
        IConteudoSiteService conteudos,
        IArquivoService arquivos,
        IEstatisticaService estatisticas,
        IPontoTuristicoService pontos,
        IEventoService eventos,
        INoticiaService noticias,
        IRoteiroService roteiros,
        IContatoService contatos,
        IConhecaEstanciaService conheca,
        IConfiguracaoSiteService configuracoes)
        : base(services)
    {
        _conteudos = conteudos;
        _arquivos = arquivos;
        _estatisticas = estatisticas;
        _pontos = pontos;
        _eventos = eventos;
        _noticias = noticias;
        _roteiros = roteiros;
        _contatos = contatos;
        _conheca = conheca;
        _configuracoes = configuracoes;
    }

    // ===== Hero =====
    public async Task<IActionResult> Hero(CancellationToken ct)
    {
        ViewData["Title"] = "Hero";
        var vm = await MontarAsync("hero", "Hero", "Abertura do portal: título, chamada e vídeo de fundo.", ct,
            textos: [T("hero-titulo", "Título", "Ex.: Explore as Cores, a História e a Tradição de Estância"), T("hero-subtitulo", "Subtítulo", "Frase curta sob o título; quebras de linha viram <br>")],
            imagens: [],
            ancora: "#section-cidade");
        vm.VideoArquivoId = (await _configuracoes.ObterPorChaveAsync("video-institucional", ct))?.ArquivoId;
        vm.VideoArquivoIdMobile = (await _configuracoes.ObterPorChaveAsync("video-institucional-mobile", ct))?.ArquivoId;
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
    public async Task<IActionResult> SalvarVideoHero(IFormFile? arquivo, CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            TempData["PainelErro"] = "Selecione um arquivo de vídeo (MP4).";
            return RedirectToAction(nameof(Hero));
        }

        var atual = await _configuracoes.ObterPorChaveAsync("video-institucional", ct);
        var dto = atual ?? new ConfiguracaoSiteDto
        {
            Chave = "video-institucional",
            Nome = "Vídeo institucional",
            Tipo = TipoConfiguracao.Arquivo
        };
        await _configuracoes.SalvarAsync(dto, arquivo, ct);
        TempData["PainelOk"] = "Vídeo institucional atualizado.";
        return RedirectToAction(nameof(Hero));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarVideoHeroMobile(IFormFile? arquivo, CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            TempData["PainelErro"] = "Selecione um arquivo de vídeo para mobile (MP4).";
            return RedirectToAction(nameof(Hero));
        }

        var atual = await _configuracoes.ObterPorChaveAsync("video-institucional-mobile", ct);
        var dto = atual ?? new ConfiguracaoSiteDto
        {
            Chave = "video-institucional-mobile",
            Nome = "Vídeo do hero — mobile",
            Tipo = TipoConfiguracao.Arquivo
        };
        await _configuracoes.SalvarAsync(dto, arquivo, ct);
        TempData["PainelOk"] = "Vídeo mobile atualizado.";
        return RedirectToAction(nameof(Hero));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverVideoHeroMobile(CancellationToken ct)
    {
        var atual = await _configuracoes.ObterPorChaveAsync("video-institucional-mobile", ct);
        if (atual is not null)
        {
            await _configuracoes.ExcluirAsync(atual.Id, ct);
            TempData["PainelOk"] = "Vídeo mobile removido — o hero usará o vídeo desktop no celular.";
        }
        return RedirectToAction(nameof(Hero));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverVideoHero(CancellationToken ct)
    {
        var atual = await _configuracoes.ObterPorChaveAsync("video-institucional", ct);
        if (atual is not null)
        {
            await _configuracoes.ExcluirAsync(atual.Id, ct);
            TempData["PainelOk"] = "Vídeo do hero removido.";
        }
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

    // ===== Conheça Estância (seção de tela cheia da home) =====
    public async Task<IActionResult> Conheca(CancellationToken ct)
    {
        ViewData["Title"] = "Conheça Estância";
        var vm = await MontarAsync("conheca", "Conheça Estância", "Seção de tela cheia da home: fotos com texto sobreposto nas abas História, Cultura, Gastronomia e Experiências. Conteúdo exclusivo desta área.", ct,
            textos: [
                T("conheca-titulo", "Título", "Aceita <strong> para destacar; ex.: Conheça <strong>Estância</strong>"),
                T("conheca-descricao", "Descrição", "Chamada da seção")
            ],
            imagens: [],
            ancora: "#section-conheca");
        var itens = await _conheca.ListarAsync(ct);
        vm.Itens = new ItensAreaViewModel
        {
            Titulo = "Itens do Conheça Estância",
            RotuloBotao = "Cadastrar novo item",
            UrlCriar = Url.Action("Criar", "ConhecaEstancia", new { area = "Gerenciador" }) ?? "/Gerenciador/ConhecaEstancia/Criar",
            UrlLista = Url.Action("Index", "ConhecaEstancia", new { area = "Gerenciador" }) ?? "/Gerenciador/ConhecaEstancia",
            Icone = "compass",
            Itens = itens.Select(i => new ItemAreaViewModel { Id = i.Id, Nome = i.Nome, Detalhe = i.Categoria.ToString(), ImagemArquivoId = i.ImagemArquivoId, Ativo = i.Ativo }).ToList()
        };
        return View("Editar", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarConheca(AreaSiteViewModel vm, CancellationToken ct)
    {
        await SalvarAsync(vm, ct);
        TempData["PainelOk"] = "Conheça Estância atualizada.";
        return RedirectToAction(nameof(Conheca));
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
        var vm = await MontarAsync("mapa", "Mapa interativo", "Imagem do município (PNG) + chamada do mapa e acesso às categorias e pontos. Os pins são os pontos com “Exibir no mapa”.", ct,
            textos: [
                T("mapa-titulo", "Título", "Aceita <span class=\"secao-destaque\"> para destacar; ex.: Encontre as <span class=\"secao-destaque\">7 Maravilhas</span>"),
                T("mapa-descricao", "Descrição", "Chamada do mapa interativo")
            ],
            imagens: [I("mapa-imagem", "Imagem do mapa (PNG do município) — sugestão: 1600×900px (16:9), PNG com fundo transparente, até 1 MB")],
            ancora: "#section-mapa");
        var dic = await _conteudos.ObterDicionarioAsync(ct);
        vm.MapaImagemZoom = dic.TryGetValue("mapa-imagem-zoom", out var z) && int.TryParse(z, out var zi) && zi is >= 100 and <= 250 ? zi : 100;
        vm.MapaImagemPosX = dic.TryGetValue("mapa-imagem-pos-x", out var x) && int.TryParse(x, out var xi) && xi is >= 0 and <= 100 ? xi : 50;
        vm.MapaImagemPosY = dic.TryGetValue("mapa-imagem-pos-y", out var y) && int.TryParse(y, out var yi) && yi is >= 0 and <= 100 ? yi : 50;
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
        ViewBag.PontosMapaGeral = mapa;
        return View("Editar", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarMapa(AreaSiteViewModel vm, CancellationToken ct)
    {
        await SalvarAsync(vm, ct);
        // Ajuste de zoom/posição da imagem do mapa (mesma biblioteca de Notícias)
        var zoom = Request.Form.TryGetValue("MapaImagemZoom", out var zv) && int.TryParse(zv, out var zi) && zi is >= 100 and <= 250 ? zi.ToString() : "100";
        var posX = Request.Form.TryGetValue("MapaImagemPosX", out var xv) && int.TryParse(xv, out var xi) && xi is >= 0 and <= 100 ? xi.ToString() : "50";
        var posY = Request.Form.TryGetValue("MapaImagemPosY", out var yv) && int.TryParse(yv, out var yi) && yi is >= 0 and <= 100 ? yi.ToString() : "50";
        await _conteudos.SalvarPorChaveAsync("mapa-imagem-zoom", "Mapa — zoom da imagem", zoom, ct);
        await _conteudos.SalvarPorChaveAsync("mapa-imagem-pos-x", "Mapa — posição X", posX, ct);
        await _conteudos.SalvarPorChaveAsync("mapa-imagem-pos-y", "Mapa — posição Y", posY, ct);
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
