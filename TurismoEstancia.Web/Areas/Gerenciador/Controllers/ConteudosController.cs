using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Conteudo.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class ConteudosController : PainelController
{
    private readonly IConteudoSiteService _conteudos;

    public ConteudosController(IServiceProvider services, IConteudoSiteService conteudos)
        : base(services) => _conteudos = conteudos;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Textos do portal";
        return View(await _conteudos.ListarAsync(ct));
    }

    public IActionResult Criar() => View(new ConteudoSiteDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ConteudoSiteDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _conteudos.SalvarAsync(dto, ct);
            TempData["PainelOk"] = "Texto salvo.";
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
        ViewData["Title"] = "Editar texto";
        var dto = await _conteudos.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ConteudoSiteDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _conteudos.SalvarAsync(dto, ct);
            TempData["PainelOk"] = "Texto atualizado.";
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
        await _conteudos.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Texto excluído.";
        return RedirectToAction(nameof(Index));
    }
}
