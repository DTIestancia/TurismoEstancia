using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.ConhecaEstancia.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class ConhecaEstanciaController : PainelController
{
    private readonly IConhecaEstanciaService _conheca;

    public ConhecaEstanciaController(IServiceProvider services, IConhecaEstanciaService conheca)
        : base(services) => _conheca = conheca;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Conheça Estância";
        return View(await _conheca.ListarAsync(ct));
    }

    public IActionResult Criar()
    {
        ViewData["Title"] = "Novo item do Conheça Estância";
        return View(new ConhecaEstanciaItemDto());
    }

    private bool EhEmbutido() =>
        Request.Query["embutido"] == "1" ||
        Request.Form["embutido"] == "1" ||
        Request.Headers.Referer.ToString().Contains("embutido=1");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ConhecaEstanciaItemDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        await _conheca.SalvarAsync(dto, imagem, ct);
        TempData["PainelOk"] = "Item salvo no Conheça Estância.";
        if (EhEmbutido()) return RedirectToAction(nameof(Index), new { embutido = 1 });
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar item do Conheça Estância";
        var dto = await _conheca.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ConhecaEstanciaItemDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        await _conheca.SalvarAsync(dto, imagem, ct);
        TempData["PainelOk"] = "Item atualizado.";
        if (EhEmbutido()) return RedirectToAction(nameof(Index), new { embutido = 1 });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _conheca.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Item excluído do Conheça Estância.";
        return RedirectToAction(nameof(Index));
    }
}
