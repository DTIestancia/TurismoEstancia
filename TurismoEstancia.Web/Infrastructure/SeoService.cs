using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Infrastructure;

/// <summary>
/// Monta os metadados SEO de cada página: carrega os padrões das configurações
/// do site (título, meta descrição, logotipo) uma vez por request e aplica o
/// override informado pela página (ViewData["Seo"]).
/// </summary>
public class SeoService
{
    private readonly IConfiguracaoSiteService _configs;
    private readonly ILogger<SeoService> _logger;
    private SeoMeta? _padrao;

    public SeoService(IConfiguracaoSiteService configs, ILogger<SeoService> logger)
    {
        _configs = configs;
        _logger = logger;
    }

    /// <summary>Meta final da página: padrões das configurações + override da página.</summary>
    public async Task<SeoMeta> ObterMetaAsync(SeoMeta? pagina, CancellationToken ct = default)
    {
        var padrao = await CarregarPadraoAsync(ct);

        return new SeoMeta
        {
            Titulo = pagina?.Titulo,
            Descricao = string.IsNullOrWhiteSpace(pagina?.Descricao) ? padrao.SiteDescricao : pagina!.Descricao,
            ImagemUrl = pagina?.ImagemUrl ?? padrao.ImagemUrl,
            Tipo = pagina?.Tipo ?? "website",
            DataPublicacao = pagina?.DataPublicacao,
            NoIndex = pagina?.NoIndex ?? false,
            NomeSite = padrao.NomeSite,
            SiteDescricao = padrao.SiteDescricao
        };
    }

    private async Task<SeoMeta> CarregarPadraoAsync(CancellationToken ct)
    {
        if (_padrao is not null) return _padrao;

        _padrao = new SeoMeta();
        try
        {
            var configs = await _configs.ListarAsync(ct);
            _padrao.NomeSite = configs.FirstOrDefault(c => c.Chave == "site-titulo")?.ValorTexto ?? "Descubra Estância";
            _padrao.SiteDescricao = configs.FirstOrDefault(c => c.Chave == "meta-descricao")?.ValorTexto;
            _padrao.ImagemUrl = configs.FirstOrDefault(c => c.Chave == "logo")?.ArquivoId is long id
                ? $"/arquivo/{id}"
                : null;
        }
        catch (Exception ex)
        {
            // Banco indisponível (ex.: primeiro boot sem migração): fica com os defaults.
            _logger.LogWarning(ex, "SEO: falha ao carregar as configurações do site.");
        }

        return _padrao;
    }
}
