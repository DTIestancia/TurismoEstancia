using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Planeje.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class PlanejeCategoriasController : PainelController
{
    private readonly IPlanejeService _planeje;

    public PlanejeCategoriasController(IServiceProvider services, IPlanejeService planeje)
        : base(services) => _planeje = planeje;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Categorias do Planeje";
        ViewData["AreaAtiva"] = "roteiros";
        return View(await _planeje.ListarCategoriasAsync(false, ct));
    }

    public IActionResult Criar()
    {
        ViewData["Title"] = "Nova categoria";
        ViewData["AreaAtiva"] = "roteiros";
        return View(new PlanejeCategoriaDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(PlanejeCategoriaDto dto, IFormFile? imagemPadrao, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _planeje.SalvarCategoriaAsync(dto, imagemPadrao, ct);
            TempData["PainelOk"] = "Categoria salva.";
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
        ViewData["Title"] = "Editar categoria";
        ViewData["AreaAtiva"] = "roteiros";
        var dto = await _planeje.ObterCategoriaAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(PlanejeCategoriaDto dto, IFormFile? imagemPadrao, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _planeje.SalvarCategoriaAsync(dto, imagemPadrao, ct);
            TempData["PainelOk"] = "Categoria atualizada.";
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
            await _planeje.OcultarCategoriaAsync(id, ct);
            TempData["PainelOk"] = "Categoria ocultada.";
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
            await _planeje.ReativarCategoriaAsync(id, ct);
            TempData["PainelOk"] = "Categoria reativada.";
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
            await _planeje.ExcluirCategoriaAsync(id, ct);
            TempData["PainelOk"] = "Categoria excluída.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirecionarParaIndex();
    }
}
