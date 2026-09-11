using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TurismoEstancia.Authorization.Services;
using TurismoEstancia.Services.Avaliacao.Interfaces;

namespace TurismoEstancia.Web.Areas.Operador.Controllers;

/// <summary>Base da área Operador (Evento + Newsletter apenas).</summary>
[Area("Operador")]
[Authorize(Policy = Perfis.Operador)]
public abstract class OperadorController : Controller
{
    private readonly IServiceProvider _services;

    protected OperadorController(IServiceProvider services) => _services = services;

    /// <summary>
    /// True quando a requisição veio do modal de cadastro (iframe ?embutido=1).
    /// Cobre GET (query), POST (campo oculto do form) e o redirect intermediário (referer).
    /// </summary>
    protected bool EhEmbutido() =>
        Request.Query["embutido"] == "1" ||
        Request.Form["embutido"] == "1" ||
        Request.Headers.Referer.ToString().Contains("embutido=1");

    /// <summary>
    /// Volta para a Index preservando ?embutido=1 quando no modal, para o
    /// layout emitir o postMessage que fecha o dialog e recarrega a lista.
    /// Fora do modal comporta-se como o redirect normal.
    /// </summary>
    protected IActionResult RedirecionarParaIndex() =>
        EhEmbutido() ? RedirectToAction("Index", new { embutido = 1 }) : RedirectToAction("Index");

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        using var scope = _services.CreateScope();
        var avaliacoes = scope.ServiceProvider.GetRequiredService<IAvaliacaoService>();
        ViewData["PendentesAvaliacoes"] = await avaliacoes.ContarPendentesAsync();
        await next();
    }
}
