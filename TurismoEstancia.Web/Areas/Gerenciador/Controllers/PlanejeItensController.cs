using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Planeje.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class PlanejeItensController : PainelController
{
    private readonly IPlanejeService _planeje;

    public PlanejeItensController(IServiceProvider services, IPlanejeService planeje)
        : base(services) => _planeje = planeje;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Locais do Planeje";
        ViewData["AreaAtiva"] = "roteiros";
        return View(await _planeje.ListarItensAsync(false, ct));
    }

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["Title"] = "Novo local";
        ViewData["AreaAtiva"] = "roteiros";
        ViewBag.Categorias = await _planeje.ListarCategoriasAsync(false, ct);
        return View(new PlanejeItemDto { Ativo = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(PlanejeItemDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _planeje.ListarCategoriasAsync(false, ct);
            return View(dto);
        }
        try
        {
            await _planeje.SalvarItemAsync(dto, imagem, ct);
            TempData["PainelOk"] = "Local salvo.";
            return RedirecionarParaIndex();
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            ViewBag.Categorias = await _planeje.ListarCategoriasAsync(false, ct);
            return View(dto);
        }
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar local";
        ViewData["AreaAtiva"] = "roteiros";
        var dto = await _planeje.ObterItemAsync(id, ct);
        if (dto is null) return NotFound();
        ViewBag.Categorias = await _planeje.ListarCategoriasAsync(false, ct);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(PlanejeItemDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _planeje.ListarCategoriasAsync(false, ct);
            return View(dto);
        }
        try
        {
            await _planeje.SalvarItemAsync(dto, imagem, ct);
            TempData["PainelOk"] = "Local atualizado.";
            return RedirecionarParaIndex();
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            ViewBag.Categorias = await _planeje.ListarCategoriasAsync(false, ct);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _planeje.ExcluirItemAsync(id, ct);
        TempData["PainelOk"] = "Local excluído.";
        return RedirecionarParaIndex();
    }
}
