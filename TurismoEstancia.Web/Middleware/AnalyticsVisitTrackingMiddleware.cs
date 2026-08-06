using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Analytics.Interfaces;

namespace TurismoEstancia.Web.Middleware;

/// <summary>
/// Rastreia as visitas das páginas públicas do portal de forma anônima (LGPD):
/// nenhum IP ou dado pessoal é armazenado. O visitante único é identificado por
/// um cookie UUID de primeira parte (sem consentimento exigido). As visitas são
/// enfileiradas em background — o request nunca espera pelo banco.
/// </summary>
public class AnalyticsVisitTrackingMiddleware
{
    private const string CookieSessao = "te_sessao";

    /// <summary>Prefixos que não são páginas públicas (painel, mídias, API, SEO).</summary>
    private static readonly string[] PrefixosIgnorados =
    {
        "/Gerenciador", "/Operador", "/arquivo/", "/api/", "/css/", "/js/", "/lib/",
        "/img", "/images", "/favicon", "/sitemap.xml", "/robots.txt",
        "/Account", "/Identity", "/Home/Error", "/Privacy", "/Evento/"
    };

    private readonly RequestDelegate _next;

    public AnalyticsVisitTrackingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // IAnalyticsService é scoped — vem como parâmetro do InvokeAsync para ser
    // resolvido no escopo do request (middleware não recebe scoped no construtor).
    public async Task InvokeAsync(HttpContext context, IAnalyticsService analytics)
    {
        // Garante a sessão anônima ANTES do response começar (cookie no primeiro acesso).
        var sessaoId = context.Request.Cookies[CookieSessao];
        if (string.IsNullOrEmpty(sessaoId))
        {
            sessaoId = Guid.NewGuid().ToString("N");
            context.Response.Cookies.Append(CookieSessao, sessaoId, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                MaxAge = TimeSpan.FromDays(400)
            });
        }

        await _next(context);

        // Só registra páginas servidas com sucesso (GET, sem redirecionamento/erro).
        if (context.Request.Method != HttpMethods.Get) return;
        if (context.Response.StatusCode < 200 || context.Response.StatusCode >= 400) return;

        var rota = context.Request.Path.Value ?? "";
        if (!EhPaginaPublica(rota)) return;

        analytics.Registrar(new AnalyticsEventoDto
        {
            Rota = rota,
            Titulo = TitularPagina(rota),
            RefererHost = ExtrairRefererHost(context),
            SessaoId = sessaoId,
            Dispositivo = DetectarDispositivo(context.Request.Headers.UserAgent.ToString())
        });
    }

    private static bool EhPaginaPublica(string rota)
    {
        if (rota == "/") return true;
        foreach (var prefixo in PrefixosIgnorados)
        {
            if (rota.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase))
                return false;
        }
        return true;
    }

    /// <summary>Título amigável da página, por convenção de rota.</summary>
    private static string TitularPagina(string rota)
    {
        if (rota == "/") return "Início";
        var partes = rota.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return "Início";

        return partes[0] switch
        {
            "cidade" => "Nossa Cidade",
            "cultura" => partes.Length > 1 ? "Nossa Cultura — Detalhe" : "Nossa Cultura",
            "grupos-populares" => partes.Length > 1 ? "Grupos Populares — Detalhe" : "Grupos Populares",
            "gastronomia" => partes.Length > 1 ? "Gastronomia — Detalhe" : "Gastronomia",
            "lugares" => partes.Length > 1 ? "Lugares que Encantam — Detalhe" : "Lugares que Encantam",
            "noticias" => partes.Length > 1 ? "Notícia" : "Notícias",
            "roteiros" => partes.Length > 1 ? "Roteiro" : "Roteiros",
            "agenda" => "Agenda",
            "contato" => "Contato",
            "Evento" => "Evento",
            _ => "Portal"
        };
    }

    private static string DetectarDispositivo(string? userAgent)
    {
        var ua = (userAgent ?? "").ToLowerInvariant();
        if (ua.Contains("ipad") || ua.Contains("tablet")) return "Tablet";
        if (ua.Contains("android") || ua.Contains("mobile") || ua.Contains("iphone")) return "Mobile";
        return "Desktop";
    }

    /// <summary>Domínio de origem (referer). Mesmo host = acesso direto (não registra).</summary>
    private static string? ExtrairRefererHost(HttpContext context)
    {
        var referer = context.Request.Headers.Referer.ToString();
        if (string.IsNullOrWhiteSpace(referer)) return null;
        if (!Uri.TryCreate(referer, UriKind.Absolute, out var uri)) return null;
        if (string.Equals(uri.Host, context.Request.Host.Host, StringComparison.OrdinalIgnoreCase)) return null;
        return uri.Host;
    }
}
