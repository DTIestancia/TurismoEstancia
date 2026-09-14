using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace TurismoEstancia.Web.Controllers;

/// <summary>
/// Entrega um token anti-forgery novo para a página atual.
///
/// Por que existe: o token é amarrado ao cookie de quem gerou o HTML, e agora as
/// páginas do portal podem ser servidas do cache (ver <c>CachePortalPublicoPolicy</c>) —
/// o token que vem no HTML pode ser de outro visitante e o POST seria recusado.
/// O portal.js busca este endpoint no carregamento e injeta o token em todos os
/// formulários da página; o campo que veio no HTML continua valendo como fallback
/// para quem não executa JS.
///
/// A rota está na lista de "não públicas" (RotasPortal): nunca é cacheada e não
/// conta como visita.
/// </summary>
public class AntiforgeryController : Controller
{
    private readonly IAntiforgery _antiforgery;

    public AntiforgeryController(IAntiforgery antiforgery) => _antiforgery = antiforgery;

    /// <summary>GET /antiforgery/token — grava o cookie e devolve o token da requisição.</summary>
    [HttpGet]
    [Route("antiforgery/token")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Token()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        return Json(new { token = tokens.RequestToken });
    }
}
