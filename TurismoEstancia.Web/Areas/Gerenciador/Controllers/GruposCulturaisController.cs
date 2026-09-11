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
        try
        {
            await _grupos.SalvarAsync(dto, imagem, ct);
            TempData["PainelOk"] = "Grupo cultural salvo.";
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
        ViewData["Title"] = "Editar grupo cultural";
        var dto = await _grupos.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(GrupoCulturalDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _grupos.SalvarAsync(dto, imagem, ct);
            TempData["PainelOk"] = "Grupo cultural atualizado.";
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
            await _grupos.OcultarAsync(id, ct);
            TempData["PainelOk"] = "Grupo cultural ocultado.";
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
            await _grupos.ReativarAsync(id, ct);
            TempData["PainelOk"] = "Grupo cultural reativado.";
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
            await _grupos.ExcluirAsync(id, ct);
            TempData["PainelOk"] = "Grupo cultural excluído.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirecionarParaIndex();
    }
}
