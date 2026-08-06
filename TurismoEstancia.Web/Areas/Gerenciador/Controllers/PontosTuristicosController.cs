using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class PontosTuristicosController : PainelController
{
    private readonly IPontoTuristicoService _pontos;
    private readonly ICategoriaPontoTuristicoService _categorias;

    public PontosTuristicosController(
        IServiceProvider services,
        IPontoTuristicoService pontos,
        ICategoriaPontoTuristicoService categorias)
        : base(services)
    {
        _pontos = pontos;
        _categorias = categorias;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Pontos turísticos";
        return View(await _pontos.ListarAsync(apenasAtivos: false, ct));
    }

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["Title"] = "Novo ponto turístico";
        ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
        return View(new PontoTuristicoDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(
        PontoTuristicoDto dto,
        IFormFile? capa,
        IFormFile? pictograma,
        IEnumerable<IFormFile> galeria,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
            return View(dto);
        }

        try
        {
            await _pontos.SalvarAsync(dto, capa, pictograma, galeria, ct);
            TempData["PainelOk"] = "Ponto turístico salvo.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
            return View(dto);
        }
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar ponto turístico";
        ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
        var dto = await _pontos.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        PontoTuristicoDto dto,
        IFormFile? capa,
        IFormFile? pictograma,
        IEnumerable<IFormFile> galeria,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
            return View(dto);
        }

        try
        {
            await _pontos.SalvarAsync(dto, capa, pictograma, galeria, ct);
            TempData["PainelOk"] = "Ponto turístico atualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _pontos.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Ponto turístico desativado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reativar(int id, CancellationToken ct)
    {
        await _pontos.ReativarAsync(id, ct);
        TempData["PainelOk"] = "Ponto turístico reativado.";
        return RedirectToAction(nameof(Index));
    }
}
