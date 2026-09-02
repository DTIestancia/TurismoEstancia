using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Services.Infra.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class ConteudosController : PainelController
{
    private readonly IConteudoSiteService _conteudos;
    private readonly IArquivoService _arquivos;

    public ConteudosController(IServiceProvider services, IConteudoSiteService conteudos, IArquivoService arquivos)
        : base(services)
    {
        _conteudos = conteudos;
        _arquivos = arquivos;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Textos do portal";

        var cadastrados = (await _conteudos.ListarAsync(ct))
            .ToDictionary(c => c.Chave, c => c);

        var vm = new ConteudosCatalogViewModel
        {
            // Chaves lidas pelo portal (views da home e páginas internas), com
            // o texto padrão do protótipo quando a chave ainda não foi criada.
            // Nomes são exibidos por seção para o Gerenciador não precisar
            // decorar as chaves (ex.: seção Nossa Cidade = historia-*).
            Secoes =
            [
                new ConteudosCatalogViewModel.SecaoTextos
                {
                    Titulo = "Hero (primeira tela)",
                    Descricao = "Título e chamada da abertura do portal.",
                    Itens = Itens(cadastrados,
                        ("hero-titulo", "Título do hero", "Ex.: Explore as Cores, a História e a Tradição de Estância"),
                        ("hero-subtitulo", "Subtítulo do hero", "Frase curta sob o título, com quebra de linha <br>"))
                },
                new ConteudosCatalogViewModel.SecaoTextos
                {
                    Titulo = "Nossa Cidade",
                    Descricao = "A seção de história da cidade (chaves historia-*).",
                    Itens = Itens(cadastrados,
                        ("historia-texto", "Texto da história", "Parágrafo principal; aceita <strong> e <br>"),
                        ("historia-citacao", "Citação", "Frase de efeito entre aspas"),
                        ("historia-descricao", "Descrição", "Segundo parágrafo (lado direito); aceita <strong>"),
                        ("historia-imagem", "Imagem da cidade", "Id de arquivo enviado em Arquivos (ex.: 12)"))
                },
                new ConteudosCatalogViewModel.SecaoTextos
                {
                    Titulo = "Conheça Estância",
                    Descricao = "Explorador da home (História, Cultura, Gastronomia, Experiências).",
                    Itens = Itens(cadastrados,
                        ("conheca-titulo", "Título", "Aceita <strong> para destacar; ex.: Conheça <strong>Estância</strong>"),
                        ("conheca-descricao", "Descrição", "Chamada da seção"))
                },
                new ConteudosCatalogViewModel.SecaoTextos
                {
                    Titulo = "7 Maravilhas",
                    Descricao = "Seção do baralho de cartas das 7 Maravilhas.",
                    Itens = Itens(cadastrados,
                        ("maravilhas-titulo", "Título", "Aceita <span class=\"vitrine-accent\"> para destacar"),
                        ("maravilhas-descricao", "Descrição", "Chamada acima do deck"))
                },
                new ConteudosCatalogViewModel.SecaoTextos
                {
                    Titulo = "Agenda",
                    Descricao = "Programação oficial de eventos.",
                    Itens = Itens(cadastrados,
                        ("agenda-titulo", "Título", "Aceita <br> para quebrar linha"),
                        ("agenda-subtitulo", "Subtítulo", "Frase sob o título"),
                        ("agenda-pill", "Selo da agenda", "Ex.: Programação Oficial"))
                },
                new ConteudosCatalogViewModel.SecaoTextos
                {
                    Titulo = "Notícias, Roteiros e Mapa",
                    Descricao = "Chamadas das seções de conteúdo dinâmico.",
                    Itens = Itens(cadastrados,
                        ("noticias-titulo", "Título das notícias", "Aceita <span class=\"secao-destaque\"> para destacar"),
                        ("noticias-descricao", "Descrição das notícias", "Chamada da seção de notícias"),
                        ("roteiros-titulo", "Título dos roteiros", "Aceita <span class=\"secao-destaque\"> para destacar"),
                        ("roteiros-descricao", "Descrição dos roteiros", "Chamada da seção de roteiros"),
                        ("mapa-titulo", "Título do mapa", "Aceita <span class=\"secao-destaque\"> para destacar"),
                        ("mapa-descricao", "Descrição do mapa", "Chamada do mapa interativo"))
                },
                new ConteudosCatalogViewModel.SecaoTextos
                {
                    Titulo = "Newsletter (rodapé)",
                    Descricao = "Chamada do formulário de e-mail.",
                    Itens = Itens(cadastrados,
                        ("newsletter-titulo", "Título da newsletter", "Chamada acima do campo de e-mail"))
                }
            ]
        };

        return View(vm);
    }

    private static ConteudosCatalogViewModel.ItemTexto[] Itens(
        IReadOnlyDictionary<string, ConteudoSiteDto> cadastrados,
        params (string Chave, string Nome, string? Descricao)[] specs) =>
        specs.Select(s =>
        {
            cadastrados.TryGetValue(s.Chave, out var c);
            return new ConteudosCatalogViewModel.ItemTexto
            {
                Chave = s.Chave,
                Nome = c?.Nome ?? s.Nome,
                Descricao = s.Descricao,
                Texto = c?.Texto,
                Id = c?.Id
            };
        }).ToArray();

    public async Task<IActionResult> Criar(string? chave = null, string? nome = null, CancellationToken ct = default)
    {
        ViewData["Title"] = "Novo texto";
        await PreencherChavesAsync(ViewData, ct);
        return View(new ConteudoSiteDto { Chave = chave ?? string.Empty, Nome = nome ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ConteudoSiteDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PreencherChavesAsync(ViewData, ct);
            return View(dto);
        }
        try
        {
            var antigoId = await AplicarImagemAsync(dto, imagem, ct);
            await _conteudos.SalvarAsync(dto, ct);
            // Remove a imagem antiga só após o commit do novo valor.
            if (antigoId.HasValue)
                await _arquivos.ExcluirAsync(antigoId.Value, ct);
            TempData["PainelOk"] = "Texto salvo.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            await PreencherChavesAsync(ViewData, ct);
            return View(dto);
        }
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar texto";
        var dto = await _conteudos.ObterPorIdAsync(id, ct);
        if (dto is null) return NotFound();
        await PreencherChavesAsync(ViewData, ct, dto.Chave);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ConteudoSiteDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PreencherChavesAsync(ViewData, ct, dto.Chave);
            return View(dto);
        }
        try
        {
            var antigoId = await AplicarImagemAsync(dto, imagem, ct);
            await _conteudos.SalvarAsync(dto, ct);
            // Remove a imagem antiga só após o commit do novo valor.
            if (antigoId.HasValue)
                await _arquivos.ExcluirAsync(antigoId.Value, ct);
            TempData["PainelOk"] = "Texto atualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            await PreencherChavesAsync(ViewData, ct, dto.Chave);
            return View(dto);
        }
    }

    /// <summary>
    /// Chaves que guardam um id de arquivo (imagem) em vez de texto livre:
    /// o campo da tela vira upload e o id do arquivo é salvo no texto.
    /// </summary>
    private static readonly HashSet<string> ChavesImagem = new(StringComparer.OrdinalIgnoreCase)
    {
        "historia-imagem"
    };

    /// <summary>
    /// Salva o upload como o valor da chave de imagem. Retorna o id do arquivo
    /// antigo (quando a chave já tinha uma imagem) para o chamador excluí-lo
    /// após o commit — antes isso vazava um binário órfão a cada troca.
    /// </summary>
    private async Task<long?> AplicarImagemAsync(ConteudoSiteDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ChavesImagem.Contains(dto.Chave) || imagem is null || imagem.Length == 0)
            return null;

        var antigoId = dto.Id != 0 && long.TryParse(dto.Texto, out var antigo) && antigo > 0
            ? antigo
            : (long?)null;

        var id = await _arquivos.SalvarAsync(imagem, ct);
        dto.Texto = id.ToString();
        return antigoId;
    }

    /// <summary>
    /// Todas as chaves lidas pelo portal (home e páginas internas), para o
    /// Gerenciador escolher no select em vez de digitar a chave de memória.
    /// Chaves já cadastradas por outro registro aparecem desabilitadas (EmUso).
    /// </summary>
    private async Task PreencherChavesAsync(ViewDataDictionary viewData, CancellationToken ct, string? selecionada = null)
    {
        var emUso = (await _conteudos.ListarAsync(ct))
            .Where(c => c.Chave != selecionada)
            .Select(c => c.Chave)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        viewData["ChavesDisponiveis"] = new List<ChaveDisponivelViewModel>
        {
            new() { Chave = "hero-titulo", Nome = "Título do hero", EmUso = emUso.Contains("hero-titulo") },
            new() { Chave = "hero-subtitulo", Nome = "Subtítulo do hero", EmUso = emUso.Contains("hero-subtitulo") },
            new() { Chave = "historia-texto", Nome = "Nossa Cidade — texto da história", EmUso = emUso.Contains("historia-texto") },
            new() { Chave = "historia-citacao", Nome = "Nossa Cidade — citação", EmUso = emUso.Contains("historia-citacao") },
            new() { Chave = "historia-descricao", Nome = "Nossa Cidade — descrição", EmUso = emUso.Contains("historia-descricao") },
            new() { Chave = "historia-imagem", Nome = "Nossa Cidade — foto da cidade", EmUso = emUso.Contains("historia-imagem"), EhImagem = true },
            new() { Chave = "maravilhas-titulo", Nome = "7 Maravilhas — título", EmUso = emUso.Contains("maravilhas-titulo") },
            new() { Chave = "maravilhas-descricao", Nome = "7 Maravilhas — descrição", EmUso = emUso.Contains("maravilhas-descricao") },
            new() { Chave = "agenda-titulo", Nome = "Agenda — título", EmUso = emUso.Contains("agenda-titulo") },
            new() { Chave = "agenda-subtitulo", Nome = "Agenda — subtítulo", EmUso = emUso.Contains("agenda-subtitulo") },
            new() { Chave = "agenda-pill", Nome = "Agenda — selo", EmUso = emUso.Contains("agenda-pill") },
            new() { Chave = "noticias-titulo", Nome = "Notícias — título", EmUso = emUso.Contains("noticias-titulo") },
            new() { Chave = "noticias-descricao", Nome = "Notícias — descrição", EmUso = emUso.Contains("noticias-descricao") },
            new() { Chave = "roteiros-titulo", Nome = "Roteiros — título", EmUso = emUso.Contains("roteiros-titulo") },
            new() { Chave = "roteiros-descricao", Nome = "Roteiros — descrição", EmUso = emUso.Contains("roteiros-descricao") },
            new() { Chave = "mapa-titulo", Nome = "Mapa — título", EmUso = emUso.Contains("mapa-titulo") },
            new() { Chave = "mapa-descricao", Nome = "Mapa — descrição", EmUso = emUso.Contains("mapa-descricao") },
            new() { Chave = "newsletter-titulo", Nome = "Newsletter — título", EmUso = emUso.Contains("newsletter-titulo") }
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _conteudos.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Texto excluído.";
        return RedirectToAction(nameof(Index));
    }
}
