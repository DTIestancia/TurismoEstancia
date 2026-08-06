using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Web.Areas.Operador.Controllers;

public class EventosController : OperadorController
{
    private readonly IEventoService _eventos;

    public EventosController(IServiceProvider services, IEventoService eventos)
        : base(services) => _eventos = eventos;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Eventos";
        return View(await _eventos.ListarAsync(apenasProximos: false, ct));
    }

    public IActionResult Criar() => View(new EventoDto { DataInicio = DateTime.Today, DataFim = DateTime.Today });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(EventoDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        if (dto.DataFim < dto.DataInicio)
        {
            ModelState.AddModelError("DataFim", "A data de término deve ser posterior ao início.");
            return View(dto);
        }

        await _eventos.SalvarAsync(dto, ct);
        TempData["PainelOk"] = "Evento salvo.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar evento";
        var dto = await _eventos.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EventoDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        if (dto.DataFim < dto.DataInicio)
        {
            ModelState.AddModelError("DataFim", "A data de término deve ser posterior ao início.");
            return View(dto);
        }

        await _eventos.SalvarAsync(dto, ct);
        TempData["PainelOk"] = "Evento atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _eventos.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Evento excluído.";
        return RedirectToAction(nameof(Index));
    }
}
