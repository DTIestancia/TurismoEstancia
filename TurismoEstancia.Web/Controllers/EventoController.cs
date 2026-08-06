using System.Text;
using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Web.Controllers;

/// <summary>Endpoints públicos de eventos (exportação .ics para calendários).</summary>
public class EventoController : Controller
{
    private readonly IEventoService _eventos;

    public EventoController(IEventoService eventos) => _eventos = eventos;

    /// <summary>GET /Evento/{id}/ics — baixa o evento no formato iCalendar.</summary>
    [Route("Evento/{id:int}/ics")]
    public async Task<IActionResult> Ics(int id, CancellationToken ct)
    {
        try
        {
            var ics = await _eventos.GerarIcsAsync(id, ct);
            return File(Encoding.UTF8.GetBytes(ics), "text/calendar; charset=utf-8", $"evento-{id}.ics");
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
