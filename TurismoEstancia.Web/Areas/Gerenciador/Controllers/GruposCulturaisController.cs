using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class GruposCulturaisController : PainelController
{
    private readonly IGrupoCulturalService _grupos;

    public GruposCulturaisController(IServiceProvider services, IGrupoCulturalService grupos)
        : base(services) => _grupos = grupos;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Grupos culturais";
        return View(await _grupos.ListarAsync(ct));
    }

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["Title"] = "Novo grupo cultural";
        return View(new GrupoCulturalDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(GrupoCulturalDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        await _grupos.SalvarAsync(dto, imagem, ct);
        TempData["PainelOk"] = "Grupo cultural salvo.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar grupo cultural";
        var dto = await _grupos.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(GrupoCulturalDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        await _grupos.SalvarAsync(dto, imagem, ct);
        TempData["PainelOk"] = "Grupo cultural atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _grupos.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Grupo cultural excluído.";
        return RedirectToAction(nameof(Index));
    }
}
