using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class PratosTuristicosController : PainelController
{
    private readonly IPratoTuristicoService _pratos;

    public PratosTuristicosController(IServiceProvider services, IPratoTuristicoService pratos)
        : base(services) => _pratos = pratos;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Pratos turísticos";
        return View(await _pratos.ListarAsync(ct));
    }

    public IActionResult Criar() => View(new PratoTuristicoDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(PratoTuristicoDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        await _pratos.SalvarAsync(dto, imagem, ct);
        TempData["PainelOk"] = "Prato turístico salvo.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar prato turístico";
        var dto = await _pratos.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(PratoTuristicoDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        await _pratos.SalvarAsync(dto, imagem, ct);
        TempData["PainelOk"] = "Prato turístico atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _pratos.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Prato turístico excluído.";
        return RedirectToAction(nameof(Index));
    }
}
