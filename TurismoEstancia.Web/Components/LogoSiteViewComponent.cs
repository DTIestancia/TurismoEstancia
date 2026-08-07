using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Conteudo.Interfaces;

namespace TurismoEstancia.Web.Components;

/// <summary>
/// Renderiza o logotipo do portal (navbar, headers e rodapés) a partir da
/// configuração <c>logo-principal</c> (tipo Arquivo) gerenciada no Gerenciador.
/// Sem a configuração, usa a assinatura padrão que acompanha o projeto.
/// </summary>
public class LogoSiteViewComponent : ViewComponent
{
    /// <summary>Chave da configuração que armazena o logotipo enviado pelo Gerenciador.</summary>
    public const string ChaveLogo = "logo-principal";

    /// <summary>Assinatura padrão usada quando nenhum logotipo foi configurado.</summary>
    public const string Fallback = "~/img/ASSINATURA-CAPITAL-MARAVILHAS-AZUL.png";

    private readonly IConfiguracaoSiteService _configuracoes;

    public LogoSiteViewComponent(IConfiguracaoSiteService configuracoes) => _configuracoes = configuracoes;

    /// <param name="altura">Altura do logotipo em px (o navbar usa 36, headers internos 34, rodapés 30).</param>
    /// <param name="alt">Texto alternativo; padrão "Descubra Estância".</param>
    public async Task<IViewComponentResult> InvokeAsync(int altura = 36, string? alt = null)
    {
        var logo = await _configuracoes.ObterPorChaveAsync(ChaveLogo, HttpContext.RequestAborted);

        var url = logo?.ArquivoId is long arquivoId
            ? $"/arquivo/{arquivoId}"
            : Url.Content(Fallback);

        return View(new LogoSiteModel
        {
            Url = url,
            Alt = string.IsNullOrWhiteSpace(alt) ? "Descubra Estância" : alt,
            Altura = altura
        });
    }

    /// <summary>Dados exibidos na view do componente.</summary>
    public sealed class LogoSiteModel
    {
        public string Url { get; init; } = null!;
        public string Alt { get; init; } = "Descubra Estância";
        public int Altura { get; init; }
    }
}
