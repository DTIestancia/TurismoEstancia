using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Galeria.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

/// <summary>
/// Galeria de Estância no painel: CRUD de categorias dinâmicas + gestão de
/// fotos (upload múltiplo otimizado, legenda, visibilidade, ordem e exclusão).
/// </summary>
public class GaleriaController : PainelController
{
    private readonly IGaleriaService _galeria;

    public GaleriaController(IServiceProvider services, IGaleriaService galeria)
        : base(services) => _galeria = galeria;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Galeria";
        return View(await _galeria.ListarCategoriasAsync(incluirInativas: true, ct));
    }

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["Title"] = "Nova categoria da galeria";
        return View(new GaleriaCategoriaDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(GaleriaCategoriaDto dto, IFormFile? capa, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _galeria.SalvarCategoriaAsync(dto, capa, ct);
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
        var dto = await _galeria.ObterCategoriaPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(GaleriaCategoriaDto dto, IFormFile? capa, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _galeria.SalvarCategoriaAsync(dto, capa, ct);
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
            await _galeria.ExcluirCategoriaAsync(id, ct);
            TempData["PainelOk"] = "Categoria excluída (fotos removidas).";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>GET Fotos/{id} — gestão das fotos da categoria (upload, vínculos, ordem, legenda).</summary>
    public async Task<IActionResult> Fotos(int id, CancellationToken ct)
    {
        var categoria = await _galeria.ObterCategoriaPorIdAsync(id, ct);
        if (categoria is null) return NotFound();

        ViewData["Title"] = $"Fotos — {categoria.Nome}";
        ViewData["FotosDisponiveis"] = await _galeria.ListarFotosDisponiveisAsync(id, ct);
        return View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VincularFotos(int id, List<long> arquivoIds, CancellationToken ct)
    {
        try
        {
            await _galeria.VincularFotosAsync(id, arquivoIds, ct);
            TempData["PainelOk"] = "Fotos vinculadas à categoria — sem novo upload (imagens otimizadas reutilizadas).";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Fotos), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdicionarFotos(int id, List<IFormFile> fotos, CancellationToken ct)
    {
        try
        {
            await _galeria.AdicionarFotosAsync(id, fotos, ct);
            TempData["PainelOk"] = "Fotos adicionadas (otimizadas para o portal).";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Fotos), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarFoto(GaleriaMidiaDto dto, CancellationToken ct)
    {
        try
        {
            await _galeria.AtualizarFotoAsync(dto, ct);
            TempData["PainelOk"] = "Foto atualizada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Fotos), new { id = dto.CategoriaId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoverFoto(int id, int direcao, int categoriaId, CancellationToken ct)
    {
        try
        {
            await _galeria.MoverFotoAsync(id, direcao, ct);
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Fotos), new { id = categoriaId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirFoto(int id, int categoriaId, CancellationToken ct)
    {
        try
        {
            await _galeria.ExcluirFotoAsync(id, ct);
            TempData["PainelOk"] = "Foto removida.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Fotos), new { id = categoriaId });
    }
}
