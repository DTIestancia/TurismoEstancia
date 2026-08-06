using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Analytics.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Controllers;

/// <summary>
/// Beacon de analytics do portal: recebe cliques rastreados (via sendBeacon,
/// sem bloquear a navegação) e os enfileira. Anônimo: a sessão vem do cookie
/// te_sessao criado pelo middleware — nada pessoal trafega ou é armazenado.
/// </summary>
[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analytics;
    private readonly IHttpContextAccessor _http;

    public AnalyticsController(IAnalyticsService analytics, IHttpContextAccessor http)
    {
        _analytics = analytics;
        _http = http;
    }

    /// <summary>POST /api/analytics/event — registra um clique rastreado (204).</summary>
    [HttpPost("event")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Evento(AnalyticsCliqueDto dto)
    {
        var context = _http.HttpContext;
        if (context is null || string.IsNullOrWhiteSpace(dto.Evento)) return NoContent();

        var sessaoId = context.Request.Cookies["te_sessao"];
        if (string.IsNullOrEmpty(sessaoId)) return NoContent();

        var ua = context.Request.Headers.UserAgent.ToString();
        _analytics.Registrar(new AnalyticsEventoDto
        {
            Tipo = "Clique",
            Rota = string.IsNullOrWhiteSpace(dto.Rota) ? context.Request.Path.ToString() : dto.Rota,
            RefererHost = null,
            SessaoId = sessaoId,
            Dispositivo = DetectarDispositivo(ua),
            Evento = dto.Evento,
            EntidadeId = dto.EntidadeId,
            EntidadeNome = string.IsNullOrWhiteSpace(dto.EntidadeNome) ? null : dto.EntidadeNome
        });

        return NoContent();
    }

    private static string DetectarDispositivo(string? userAgent)
    {
        var ua = (userAgent ?? "").ToLowerInvariant();
        if (ua.Contains("ipad") || ua.Contains("tablet")) return "Tablet";
        if (ua.Contains("android") || ua.Contains("mobile") || ua.Contains("iphone")) return "Mobile";
        return "Desktop";
    }
}
