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

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        using var scope = _services.CreateScope();
        var avaliacoes = scope.ServiceProvider.GetRequiredService<IAvaliacaoService>();
        ViewData["PendentesAvaliacoes"] = await avaliacoes.ContarPendentesAsync();
        await next();
    }
}
