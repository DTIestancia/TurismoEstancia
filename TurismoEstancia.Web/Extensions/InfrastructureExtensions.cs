using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using TurismoEstancia.Mail;
using TurismoEstancia.Services.Infra.Arquivos;
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

        // Cache de memória do processo — usado pelo sitemap e pelas versões reduzidas
        // das mídias (ver ArquivoController). O limite é o que impede o cache de
        // crescer sem teto com o acervo: ao ser alcançado, o próprio MemoryCache
        // descarta as entradas menos usadas para abrir espaço (nada vai para disco).
        builder.Services.AddMemoryCache(opcoes => opcoes.SizeLimit = 96L * 1024 * 1024);

        // Metadados SEO do portal (defaults das configurações + override por página).
        builder.Services.AddScoped<SeoService>();

        // E-mail (SMTP): seção "Smtp" + fila em memória + worker em background.
        // Sem Host/RemetenteEmail configurados, o EmailSender avisa e não envia.
        builder.Services.Configure<SmtpConfig>(builder.Configuration.GetSection("Smtp"));
        builder.Services.AddScoped<IEmailSender, EmailSender>();
        builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
        builder.Services.AddHostedService<EmailBackgroundService>();

        // Duas camadas de limite no upload (ver LimitesDeUpload):
        //  - aqui, o TETO DE TRANSPORTE da requisição (Kestrel + IIS + multipart),
        //    generoso porque um formulário pode trazer várias fotos de uma vez;
        //  - no ponto único de gravação (ArquivoService), o limite POR ARQUIVO:
        //    5 MB para imagem e 10 MB para vídeo — é a regra que o operador vê.
        // Passar do teto de transporte morre em 413; o ErroDeUploadMiddleware
        // transforma isso em aviso no painel em vez de página de erro crua.
        builder.Services.Configure<KestrelServerOptions>(o =>
            o.Limits.MaxRequestBodySize = LimitesDeUpload.TetoDeTransporteBytes);
        builder.Services.Configure<IISServerOptions>(o =>
            o.MaxRequestBodySize = LimitesDeUpload.TetoDeTransporteBytes);
        builder.Services.Configure<FormOptions>(o =>
        {
            o.ValueLengthLimit = int.MaxValue;
            o.MultipartBodyLengthLimit = LimitesDeUpload.TetoDeTransporteBytes;
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
