using Microsoft.AspNetCore.Localization;
using System.Globalization;
using TurismoEstancia.Web.Middleware;

namespace TurismoEstancia.Web.Extensions;

/// <summary>
/// Pipeline de middleware (cultura pt-BR, static files, auth) e mapeamento de rotas.
/// </summary>
public static class PipelineExtensions
{
    public static void UseStandardPipeline(this WebApplication app)
    {
        // Cultura pt-BR (vírgula como decimal no model binding)
        var ptBr = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = ptBr;
        CultureInfo.DefaultThreadCurrentUICulture = ptBr;

        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("pt-BR"),
            SupportedCultures = new[] { ptBr },
            SupportedUICultures = new[] { ptBr }
        });

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        // Favicon dinâmico (usa a logo de logo-principal quando configurada).
        app.UseMiddleware<FaviconMiddleware>();

        app.UseResponseCompression();

        // Cache do navegador para os estáticos: sem Cache-Control o browser
        // revalida TODOS os arquivos (CSS, JS, PNGs do rodapé) a cada navegação.
        // Os assets carregados com asp-append-version (?v=hash) nunca mudam para
        // a mesma URL — cache de 1 ano + immutable (o browser nem revalida). Os
        // demais (imagens soltas, favicon, /lib) ficam com 1 dia, o que já corta
        // a revalidação e ainda permite trocar o arquivo sem re-deploy.
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.CacheControl = ctx.Context.Request.Query.ContainsKey("v")
                    ? "public, max-age=31536000, immutable"
                    : "public, max-age=86400";
            }
        });

        app.UseRouting();
        app.UseAuthentication();

        // Escrita no CMS (POST em /Gerenciador ou /Operador) descarta o cache do
        // portal assim que a resposta termina — a edição aparece no próximo acesso.
        // Fica antes da autorização para enxergar TODA tentativa de escrita: o
        // POST anônimo no painel termina em redirecionamento para o login, e sem
        // isso qualquer um descartaria o cache inteiro em loop.
        app.UseMiddleware<InvalidaCacheConteudoMiddleware>();

        app.UseAuthorization();

        // Rastreio de visitas das páginas públicas (anônimo, fila em background).
        // Fica FORA do cache de página de propósito: assim uma resposta servida
        // do cache também conta visita.
        app.UseMiddleware<AnalyticsVisitTrackingMiddleware>();

        // Troca os cookies que vieram com uma página servida do cache pelos deste
        // visitante, na única janela em que os cabeçalhos ainda aceitam mudança
        // (OnStarting). Fica FORA do cache: precisa rodar também no acerto.
        app.UseMiddleware<CookiesDeVisitanteMiddleware>();

        // Cache de página do portal público. Fica o mais perto possível do
        // endpoint (depois da compressão) para guardar o HTML sem compressão e
        // deixar o Content-Encoding ser resolvido a cada resposta.
        app.UseOutputCache();

        // 404/500 no visual do portal (sem redirecionamento de status).
        app.UseStatusCodePagesWithReExecute("/Home/Error");
    }

    public static void MapAllRoutes(this WebApplication app)
    {
        // Área do Gerenciador (CMS completo)
        app.MapAreaControllerRoute(
            name: "Gerenciador",
            areaName: "Gerenciador",
            pattern: "Gerenciador/{controller=Dashboard}/{action=Index}/{id?}");

        // Área do Operador (Evento + Newsletter)
        app.MapAreaControllerRoute(
            name: "Operador",
            areaName: "Operador",
            pattern: "Operador/{controller=Dashboard}/{action=Index}/{id?}");

        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();
    }
}
