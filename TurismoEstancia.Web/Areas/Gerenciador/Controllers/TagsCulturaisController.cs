using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class TagsCulturaisController : PainelController
{
    private readonly ITagCulturalService _tags;

    public TagsCulturaisController(IServiceProvider services, ITagCulturalService tags)
        : base(services) => _tags = tags;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Tags culturais";
        return View(await _tags.ListarAsync(ct));
    }

    public IActionResult Criar() => View(new TagCulturalDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(TagCulturalDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        await _tags.SalvarAsync(dto, ct);
        TempData["PainelOk"] = "Tag cultural salva.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar tag cultural";
        var dto = await _tags.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(TagCulturalDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        await _tags.SalvarAsync(dto, ct);
        TempData["PainelOk"] = "Tag cultural atualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _tags.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Tag cultural excluída.";
        return RedirectToAction(nameof(Index));
    }
}
