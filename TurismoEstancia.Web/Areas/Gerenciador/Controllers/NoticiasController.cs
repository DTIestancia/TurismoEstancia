using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.Galeria.Interfaces;
using TurismoEstancia.Web.Infrastructure;

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

    public async Task<IActionResult> Index(CancellationToken ct, int pagina = 1)
    {
        ViewData["Title"] = "Notícias";
        ViewData["AreaAtiva"] = "noticias";

        var todas = await _noticias.ListarAsync(apenasPublicadas: false, ct);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(todas.Count / (double)PaginaService.TamanhoPainel));
        var paginaAtual = Math.Clamp(pagina, 1, totalPaginas);

        ViewData["PaginaAtual"] = paginaAtual;
        ViewData["PaginasTotal"] = totalPaginas;
        return View(todas.Skip((paginaAtual - 1) * PaginaService.TamanhoPainel).Take(PaginaService.TamanhoPainel).ToList());
    }

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["Title"] = "Nova notícia";
        ViewData["AreaAtiva"] = "noticias";
        await PreencherGaleriaAsync(ViewData, ct);
        return View(new NoticiaDto { DataPublicacao = DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(NoticiaDto dto, IFormFile? imagem, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PreencherGaleriaAsync(ViewData, ct);
            return View(dto);
        }
        try
        {
            await _noticias.SalvarAsync(dto, imagem, ct);
            TempData["PainelOk"] = "Notícia salva.";
            return RedirecionarParaIndex();
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            await PreencherGaleriaAsync(ViewData, ct);
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
        if (!ModelState.IsValid)
        {
            await PreencherGaleriaAsync(ViewData, ct);
            return View(dto);
        }
        try
        {
            await _noticias.SalvarAsync(dto, imagem, ct);
            TempData["PainelOk"] = "Notícia atualizada.";
            return RedirecionarParaIndex();
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            await PreencherGaleriaAsync(ViewData, ct);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ocultar(int id, int pagina, CancellationToken ct)
    {
        try
        {
            await _noticias.OcultarAsync(id, ct);
            TempData["PainelOk"] = "Notícia ocultada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { pagina });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reativar(int id, int pagina, CancellationToken ct)
    {
        try
        {
            await _noticias.ReativarAsync(id, ct);
            TempData["PainelOk"] = "Notícia reativada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { pagina });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, int pagina, CancellationToken ct)
    {
        try
        {
            await _noticias.ExcluirAsync(id, ct);
            TempData["PainelOk"] = "Notícia excluída.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { pagina });
    }

    /// <summary>Categorias ativas da galeria para o select "Galeria relacionada".</summary>
    private async Task PreencherGaleriaAsync(ViewDataDictionary viewData, CancellationToken ct)
    {
        var categorias = await _galeria.ListarCategoriasAsync(incluirInativas: false, ct);
        viewData["GaleriaCategorias"] = categorias;
    }
}
