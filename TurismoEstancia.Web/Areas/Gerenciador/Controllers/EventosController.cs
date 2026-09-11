using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class EventosController : PainelController
{
    private readonly IEventoService _eventos;

    public EventosController(IServiceProvider services, IEventoService eventos)
        : base(services) => _eventos = eventos;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Eventos";
        ViewData["AreaAtiva"] = "agenda";
        return View(await _eventos.ListarAsync(apenasProximos: false, ct));
    }

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["Title"] = "Novo evento";
        ViewData["AreaAtiva"] = "agenda";
        return View(new EventoDto { DataInicio = DateTime.Today, DataFim = DateTime.Today });
    }

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

        try
        {
            await _eventos.SalvarAsync(dto, ct);
            TempData["PainelOk"] = "Evento salvo.";
            return RedirecionarParaIndex();
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            return View(dto);
        }
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

        try
        {
            await _eventos.SalvarAsync(dto, ct);
            TempData["PainelOk"] = "Evento atualizado.";
            return RedirecionarParaIndex();
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ocultar(int id, CancellationToken ct)
    {
        try
        {
            await _eventos.OcultarAsync(id, ct);
            TempData["PainelOk"] = "Evento ocultado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirecionarParaIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reativar(int id, CancellationToken ct)
    {
        try
        {
            await _eventos.ReativarAsync(id, ct);
            TempData["PainelOk"] = "Evento reativado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirecionarParaIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        try
        {
            await _eventos.ExcluirAsync(id, ct);
            TempData["PainelOk"] = "Evento excluído.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirecionarParaIndex();
    }
}
