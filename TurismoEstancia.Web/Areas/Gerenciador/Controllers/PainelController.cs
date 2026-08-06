using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TurismoEstancia.Authorization.Services;
using TurismoEstancia.Services.Avaliacao.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

/// <summary>
/// Base das áreas Gerenciador/Operador: exige a policy do perfil e injeta
/// no ViewData o total de avaliações pendentes (badge da sidebar).
/// </summary>
[Area("Gerenciador")]
[Authorize(Policy = Perfis.Gerenciador)]
public abstract class PainelController : Controller
{
    private readonly IServiceProvider _services;

    protected PainelController(IServiceProvider services) => _services = services;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        using var scope = _services.CreateScope();
        var avaliacoes = scope.ServiceProvider.GetRequiredService<IAvaliacaoService>();
        ViewData["PendentesAvaliacoes"] = await avaliacoes.ContarPendentesAsync();
        await next();
    }
}
