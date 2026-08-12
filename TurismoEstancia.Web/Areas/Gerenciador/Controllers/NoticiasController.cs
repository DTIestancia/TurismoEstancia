using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.Galeria.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class NoticiasController : PainelController
{
    private readonly INoticiaService _noticias;
    private readonly IGaleriaService _galeria;

    public NoticiasController(IServiceProvider services, INoticiaService noticias, IGaleriaService galeria)
        : base(services)
    {
        _noticias = noticias;
        _galeria = galeria;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Notícias";
        ViewData["AreaAtiva"] = "noticias";
        return View(await _noticias.ListarAsync(apenasPublicadas: false, ct));
    }

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["AreaAtiva"] = "noticias";
        await PreencherGaleriaAsync(ViewData, ct);
        return View(new NoticiaDto { DataPublicacao = DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(NoticiaDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _noticias.SalvarAsync(dto, imagem, ct);
            TempData["PainelOk"] = "Notícia salva.";
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
        ViewData["Title"] = "Editar notícia";
        ViewData["AreaAtiva"] = "noticias";
        var dto = await _noticias.ObterPorIdAsync(id, ct);
        if (dto is null) return NotFound();
        await PreencherGaleriaAsync(ViewData, ct);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(NoticiaDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _noticias.SalvarAsync(dto, imagem, ct);
            TempData["PainelOk"] = "Notícia atualizada.";
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
        await _noticias.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Notícia excluída.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Categorias ativas da galeria para o select "Galeria relacionada".</summary>
    private async Task PreencherGaleriaAsync(ViewDataDictionary viewData, CancellationToken ct)
    {
        var categorias = await _galeria.ListarCategoriasAsync(incluirInativas: false, ct);
        viewData["GaleriaCategorias"] = categorias;
    }
}
