using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class SlidesController : PainelController
{
    private readonly ISlideService _slides;

    public SlidesController(IServiceProvider services, ISlideService slides)
        : base(services) => _slides = slides;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Slides do hero";
        return View(await _slides.ListarAsync(ct));
    }

    public IActionResult Criar()
    {
        ViewData["Title"] = "Novo slide";
        return View(new SlideDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(SlideDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        if (imagem is null)
        {
            ModelState.AddModelError("Imagem", "Selecione uma imagem para o slide.");
            return View(dto);
        }

        try
        {
            await _slides.SalvarAsync(dto, imagem, ct);
            TempData["PainelOk"] = "Slide salvo.";
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
        ViewData["Title"] = "Editar slide";
        var dto = await _slides.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(SlideDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _slides.SalvarAsync(dto, imagem, ct);
            TempData["PainelOk"] = "Slide atualizado.";
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
            await _slides.OcultarAsync(id, ct);
            TempData["PainelOk"] = "Slide ocultado.";
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
            await _slides.ReativarAsync(id, ct);
            TempData["PainelOk"] = "Slide reativado.";
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
            await _slides.ExcluirAsync(id, ct);
            TempData["PainelOk"] = "Slide excluído.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirecionarParaIndex();
    }
}
