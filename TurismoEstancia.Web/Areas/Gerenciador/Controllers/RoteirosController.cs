using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Roteiro.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class RoteirosController : PainelController
{
    private readonly IRoteiroService _roteiros;

    public RoteirosController(IServiceProvider services, IRoteiroService roteiros)
        : base(services) => _roteiros = roteiros;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Roteiros";
        ViewData["AreaAtiva"] = "roteiros";
        return View(await _roteiros.ListarAsync(ct));
    }

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["AreaAtiva"] = "roteiros";
        return View(new RoteiroDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(RoteiroDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _roteiros.SalvarAsync(dto, imagem, ct);
            TempData["PainelOk"] = "Roteiro salvo.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            return View(dto);
        }
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar roteiro";
        ViewData["AreaAtiva"] = "roteiros";
        var dto = await _roteiros.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(RoteiroDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _roteiros.SalvarAsync(dto, imagem, ct);
            TempData["PainelOk"] = "Roteiro atualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _roteiros.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Roteiro excluído.";
        return RedirectToAction(nameof(Index));
    }
}
