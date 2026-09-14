namespace TurismoEstancia.Web.Infrastructure;

/// <summary>
/// Sessão anônima do visitante (cookie <c>te_sessao</c>) — a identidade usada
/// pelo analytics para contar visitante único, sem guardar nada pessoal (LGPD).
///
/// Vive num único lugar porque dois middlewares precisam concordar sobre ela: o
/// de rastreio de visitas cria a sessão, e o de cookies a devolve ao visitante
/// quando a página vem do cache (ver <c>CookiesDeVisitanteMiddleware</c>).
/// </summary>
public static class SessaoAnonima
{
    public const string NomeCookie = "te_sessao";

    /// <summary>Chave em <c>HttpContext.Items</c> com o id da sessão do request.</summary>
    public const string ItemSessaoId = "turismo.sessao-anonima";

    /// <summary>Opções do cookie (as mesmas na criação e na reintrodução).</summary>
    public static CookieOptions OpcoesCookie() => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        IsEssential = true,
        MaxAge = TimeSpan.FromDays(400)
    };
}
