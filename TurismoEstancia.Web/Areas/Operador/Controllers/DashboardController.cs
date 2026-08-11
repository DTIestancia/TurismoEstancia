using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.Turismo.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Areas.Operador.Controllers;

public class DashboardController : OperadorController
{
    private readonly IEventoService _eventos;
    private readonly IInscricaoNewsletterService _newsletter;

    public DashboardController(
        IServiceProvider services,
        IEventoService eventos,
        IInscricaoNewsletterService newsletter)
        : base(services)
    {
        _eventos = eventos;
        _newsletter = newsletter;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Dashboard";

        var itens = new List<PainelStatViewModel>
        {
            new() { Rotulo = "Eventos na agenda", Icone = "calendar", Valor = (await _eventos.ListarAsync(apenasProximos: false, ct)).Count },
            new() { Rotulo = "Eventos próximos", Icone = "calendar-clock", Valor = (await _eventos.ListarAsync(apenasProximos: true, ct)).Count },
            new() { Rotulo = "Inscrições newsletter", Icone = "mail", Valor = (await _newsletter.ListarAsync(incluirInativos: true, ct)).Count },
            new() { Rotulo = "Inscrições ativas", Icone = "mail-check", Valor = (await _newsletter.ListarAsync(incluirInativos: false, ct)).Count }
        };

        return View(itens);
    }
}
