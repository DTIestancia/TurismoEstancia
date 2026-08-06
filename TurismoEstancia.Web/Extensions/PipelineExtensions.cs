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

    public static async Task RunWithSeedSupportAsync(this WebApplication app, string[] args)
    {
        if (args.Contains("--seed"))
        {
            await DatabaseSeeder.SeedAsync(app);
            return;
        }

        await app.RunAsync();
    }
}
