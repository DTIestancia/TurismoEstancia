using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Web.Components;

namespace TurismoEstancia.Web.Middleware;

/// <summary>
/// Favicon dinâmico: se a configuração <c>logo-principal</c> (tipo Arquivo)
/// tiver uma imagem enviada, o <c>/favicon.ico</c> redireciona para ela;
/// caso contrário, a requisição segue para os arquivos estáticos e o
/// <c>wwwroot/favicon.ico</c> é servido como sempre foi.
/// </summary>
public class FaviconMiddleware
{
    private const string Caminho = "/favicon.ico";

    private readonly RequestDelegate _next;

    public FaviconMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // IConfiguracaoSiteService é scoped — vem como parâmetro do InvokeAsync para
    // ser resolvido no escopo do request (middleware não recebe scoped no construtor).
    public async Task InvokeAsync(HttpContext context, IConfiguracaoSiteService configuracoes)
    {
        if (context.Request.Path.Equals(Caminho, StringComparison.OrdinalIgnoreCase))
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
        }

        await _next(context);
    }
}
