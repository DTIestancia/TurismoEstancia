using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class CategoriasController : PainelController
{
    private readonly ICategoriaPontoTuristicoService _categorias;

    public CategoriasController(IServiceProvider services, ICategoriaPontoTuristicoService categorias)
        : base(services) => _categorias = categorias;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Categorias";
        return View(await _categorias.ListarAsync(incluirInativos: true, ct));
    }

    public async Task<IActionResult> Criar(CancellationToken ct) => View(new CategoriaPontoTuristicoDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CategoriaPontoTuristicoDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _categorias.SalvarAsync(dto, ct);
            TempData["PainelOk"] = "Categoria salva com sucesso.";
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
        ViewData["Title"] = "Editar categoria";
        var dto = await _categorias.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(CategoriaPontoTuristicoDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _categorias.SalvarAsync(dto, ct);
            TempData["PainelOk"] = "Categoria atualizada com sucesso.";
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
        try
        {
            await _categorias.ExcluirAsync(id, ct);
            TempData["PainelOk"] = "Categoria excluída.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
