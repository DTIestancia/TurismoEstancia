using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Web.Components;

namespace TurismoEstancia.Web.Middleware;

/// <summary>
/// Favicon dinâmico: se a configuração <c>logo-principal</c> (tipo Arquivo)
/// tiver uma imagem enviada, <c>/favicon.svg</c> (e <c>/favicon.ico</c>)
/// redirecionam para ela; caso contrário, o <c>/favicon.svg</c> estático
/// (redesenhado com as 6 cores oficiais) é servido como de costume.
/// </summary>
public class FaviconMiddleware
{
    private static readonly string[] Caminhos = ["/favicon.svg", "/favicon.ico"];

    /// <summary>
    /// Chave da configuração do favicon exclusivo do site (tipo Arquivo, PNG
    /// quadrado). Quando configurada, tem prioridade sobre o logotipo.
    /// </summary>
    public const string ChaveFavicon = "favicon";

    private readonly RequestDelegate _next;

    public FaviconMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // IConfiguracaoSiteService é scoped — vem como parâmetro do InvokeAsync para
    // ser resolvido no escopo do request (middleware não recebe scoped no construtor).
    public async Task InvokeAsync(HttpContext context, IConfiguracaoSiteService configuracoes)
    {
        if (Caminhos.Any(c => context.Request.Path.Equals(c, StringComparison.OrdinalIgnoreCase)))
        {
            // 1) Favicon exclusivo (configuração "favicon").
            var favicon = await configuracoes.ObterPorChaveAsync(
                ChaveFavicon, context.RequestAborted);
            if (favicon?.ArquivoId is long faviconId)
            {
                context.Response.StatusCode = StatusCodes.Status302Found;
                context.Response.Headers.Location = context.Request.PathBase + $"/arquivo/{faviconId}";
                context.Response.Headers.CacheControl = "public, max-age=3600";
                return;
            }

            // 2) Fallback: logotipo do portal (comportamento anterior).
            var logo = await configuracoes.ObterPorChaveAsync(
                LogoSiteViewComponent.ChaveLogo, context.RequestAborted);
            if (logo?.ArquivoId is long arquivoId)
            {
                context.Response.StatusCode = StatusCodes.Status302Found;
                context.Response.Headers.Location = context.Request.PathBase + $"/arquivo/{arquivoId}";
                // Favicon é cacheado pelo navegador; 1h mantém a troca de logo rápida.
                context.Response.Headers.CacheControl = "public, max-age=3600";
                return;
            }

            // Sem favicon nem logo configurados: /favicon.ico (navegadores antigos)
            // cai no /favicon.svg estático, que segue para os arquivos estáticos.
            if (context.Request.Path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status302Found;
                context.Response.Headers.Location = context.Request.PathBase + "/favicon.svg";
                context.Response.Headers.CacheControl = "public, max-age=86400";
                return;
            }
        }

        await _next(context);
    }
}
