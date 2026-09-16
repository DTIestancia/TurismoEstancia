using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Analytics.Interfaces;
using TurismoEstancia.Services.Busca.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Controllers;

/// <summary>Busca pública do portal (GET /buscar?q=...).</summary>
public class BuscarController : Controller
{
    private readonly IBuscaService _busca;
    private readonly IAnalyticsService _analytics;
    private readonly IHttpContextAccessor _http;

    public BuscarController(IBuscaService busca, IAnalyticsService analytics, IHttpContextAccessor http)
    {
        _busca = busca;
        _analytics = analytics;
        _http = http;
    }

    /// <summary>GET /buscar — página de resultados agrupados por seção.</summary>
    [HttpGet]
    [Route("buscar")]
    public async Task<IActionResult> Index([FromQuery] string? q, CancellationToken ct)
    {
        var vm = await _busca.BuscarAsync(q, ct);
        ViewData["Title"] = string.IsNullOrWhiteSpace(vm.Termo) ? "Buscar" : $"Busca: {vm.Termo}";
        ViewData["Seo"] = new SeoMeta
        {
            Titulo = string.IsNullOrWhiteSpace(vm.Termo) ? "Buscar no portal" : $"Busca por \"{vm.Termo}\"",
            Descricao = "Pesquise maravilhas, notícias, eventos, hospedagem, gastronomia e cultura de Estância.",
            NoIndex = true
        };

        // Registra a busca (termo) para o dashboard — mesmo padrão do beacon.
        var context = _http.HttpContext;
        var sessaoId = context?.Request.Cookies["te_sessao"];
        if (context is not null && !string.IsNullOrEmpty(sessaoId) && !string.IsNullOrWhiteSpace(vm.Termo))
        {
            var ua = context.Request.Headers.UserAgent.ToString().ToLowerInvariant();
            _analytics.Registrar(new AnalyticsEventoDto
            {
                Tipo = "Clique",
                Rota = "/buscar",
                SessaoId = sessaoId,
                Dispositivo = ua.Contains("tablet") || ua.Contains("ipad") ? "Tablet"
                    : ua.Contains("android") || ua.Contains("mobile") || ua.Contains("iphone") ? "Mobile" : "Desktop",
                Evento = "busca",
                EntidadeNome = vm.Termo.Length <= 150 ? vm.Termo : vm.Termo[..150]
            });
        }

        return View(vm);
    }
}
