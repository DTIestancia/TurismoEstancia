using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class EstatisticasController : PainelController
{
    private readonly IEstatisticaService _estatisticas;

    public EstatisticasController(IServiceProvider services, IEstatisticaService estatisticas)
        : base(services) => _estatisticas = estatisticas;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Estatísticas";
        return View(await _estatisticas.ListarAsync(ct));
    }

    public IActionResult Criar() => View(new EstatisticaDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(EstatisticaDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        await _estatisticas.SalvarAsync(dto, ct);
        TempData["PainelOk"] = "Estatística salva.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar estatística";
        var dto = await _estatisticas.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EstatisticaDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        await _estatisticas.SalvarAsync(dto, ct);
        TempData["PainelOk"] = "Estatística atualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _estatisticas.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Estatística excluída.";
        return RedirectToAction(nameof(Index));
    }
}
