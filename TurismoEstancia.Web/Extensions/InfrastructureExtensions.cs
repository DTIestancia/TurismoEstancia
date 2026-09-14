using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using TurismoEstancia.Mail;
using TurismoEstancia.Web.Infrastructure;

namespace TurismoEstancia.Web.Extensions;

/// <summary>
/// Infraestrutura transversal: MVC + Razor, limites de upload,
/// paginação e acesso ao HttpContext.
/// </summary>
public static class InfrastructureExtensions
{
    public static void AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddMemoryCache();

        // Metadados SEO do portal (defaults das configurações + override por página).
        builder.Services.AddScoped<SeoService>();

        // E-mail (SMTP): seção "Smtp" + fila em memória + worker em background.
        // Sem Host/RemetenteEmail configurados, o EmailSender avisa e não envia.
        builder.Services.Configure<SmtpConfig>(builder.Configuration.GetSection("Smtp"));
        builder.Services.AddScoped<IEmailSender, EmailSender>();
        builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
        builder.Services.AddHostedService<EmailBackgroundService>();

        // Upload limit: ~60 MB no Kestrel e IIS
        builder.Services.Configure<KestrelServerOptions>(o =>
            o.Limits.MaxRequestBodySize = 60L * 1024 * 1024);
        builder.Services.Configure<IISServerOptions>(o =>
            o.MaxRequestBodySize = 60L * 1024 * 1024);
        builder.Services.Configure<FormOptions>(o =>
        {
            o.ValueLengthLimit = int.MaxValue;
            o.MultipartBodyLengthLimit = 60L * 1024 * 1024;
        });

        // Cache de página do portal público (somente GET anônimo de página
        // pública): a regra inteira vive em CachePortalPublicoPolicy — painel,
        // Identity, APIs, mídias e POSTs ficam de fora por construção. Todas as
        // entradas saem com a tag de conteúdo, descartada a cada escrita no CMS
        // (InvalidaCacheConteudoMiddleware), então edição aparece na hora.
        builder.Services.AddOutputCache(opcoes =>
        {
            opcoes.AddBasePolicy(new CachePortalPublicoPolicy());
            // Página do portal é HTML puro; 1 MB é folga larga sobre a maior delas
            // (a home, que leva o JSON do mapa) e evita guardar resposta gigante.
            opcoes.MaximumBodySize = 1024 * 1024;
        });

        // Paginação (ReflectionIT.Mvc.Paging): o pacote moderno dispensa registro
        // em DI (Razor Class Library) — os tipos PagedList/tag helpers bastam.

        // Data Protection: chaves persistidas no SQL serão configuradas na Fase 2
        // (PersistKeysToDbContext). Sem chamada = chaves efêmeras — suficiente no esqueleto.
    }
}
