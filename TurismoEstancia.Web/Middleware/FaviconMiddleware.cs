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
            var logo = await configuracoes.ObterPorChaveAsync(
                LogoSiteViewComponent.ChaveLogo, context.RequestAborted);

            if (logo?.ArquivoId is long arquivoId)
            {
                context.Response.StatusCode = StatusCodes.Status302Found;
                context.Response.Headers.Location = $"/arquivo/{arquivoId}";
                // Favicon é cacheado pelo navegador; 1h mantém a troca de logo rápida.
                context.Response.Headers.CacheControl = "public, max-age=3600";
                return;
            }

            // Sem logo configurada: /favicon.ico (navegadores antigos) cai no
            // /favicon.svg estático, que segue para os arquivos estáticos.
            if (context.Request.Path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status302Found;
                context.Response.Headers.Location = "/favicon.svg";
                context.Response.Headers.CacheControl = "public, max-age=86400";
                return;
            }
        }

        await _next(context);
    }
}
