using System.Globalization;
using Microsoft.AspNetCore.Localization;

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
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
    }

    public static void MapAllRoutes(this WebApplication app)
    {
        // Novas áreas serão adicionadas aqui (Fase 6).
        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();
    }

    public static async Task RunWithSeedSupportAsync(this WebApplication app, string[] args)
    {
        // Suporte a --seed: implementado na Fase 7.
        await app.RunAsync();
    }
}
