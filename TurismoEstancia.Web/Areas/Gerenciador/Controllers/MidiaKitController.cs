using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.MidiaKit.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

/// <summary>CMS do Mídia kit (/midia-kit): materiais para imprensa e parceiros.</summary>
public class MidiaKitController : PainelController
{
    private readonly IMidiaKitService _midiaKit;

    public MidiaKitController(IServiceProvider services, IMidiaKitService midiaKit)
        : base(services) => _midiaKit = midiaKit;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Mídia kit";
        return View(await _midiaKit.ListarAsync(ct));
    }

    public IActionResult Criar()
    {
        ViewData["Title"] = "Novo item do mídia kit";
        return View(new MidiaKitItemDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(MidiaKitItemDto dto, IFormFile? arquivo, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _midiaKit.SalvarAsync(dto, arquivo, ct);
            TempData["PainelOk"] = "Item salvo no mídia kit.";
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
        ViewData["Title"] = "Editar item do mídia kit";
        var dto = await _midiaKit.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(MidiaKitItemDto dto, IFormFile? arquivo, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _midiaKit.SalvarAsync(dto, arquivo, ct);
            TempData["PainelOk"] = "Item atualizado.";
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
            await _midiaKit.OcultarAsync(id, ct);
            TempData["PainelOk"] = "Item ocultado.";
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
            await _midiaKit.ReativarAsync(id, ct);
            TempData["PainelOk"] = "Item reativado.";
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
            await _midiaKit.ExcluirAsync(id, ct);
            TempData["PainelOk"] = "Item excluído do mídia kit.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirecionarParaIndex();
    }
}
