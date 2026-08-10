using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Turismo.Interfaces;
using TurismoEstancia.Web.Models;

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

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["Title"] = "Nova categoria";
        await PreencherChavesAsync(ViewData, ct);
        return View(new CategoriaPontoTuristicoDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CategoriaPontoTuristicoDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PreencherChavesAsync(ViewData, ct);
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
            await PreencherChavesAsync(ViewData, ct);
            return View(dto);
        }
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar categoria";
        var dto = await _categorias.ObterPorIdAsync(id, ct);
        if (dto is null) return NotFound();
        await PreencherChavesAsync(ViewData, ct, dto.Chave);
        return View(dto);
    }

    /// <summary>
    /// Chaves das categorias já cadastradas + as do protótipo (heritage, nature,
    /// food, hotel, service) como sugestão no select; o mapa usa essas chaves.
    /// As chaves em uso por OUTRA categoria aparecem desabilitadas (EmUso).
    /// </summary>
    private async Task PreencherChavesAsync(ViewDataDictionary viewData, CancellationToken ct, string? selecionada = null)
    {
        var existentes = await _categorias.ListarAsync(incluirInativos: true, ct);
        var emUso = existentes
            .Where(c => !string.IsNullOrWhiteSpace(c.Chave) && !string.Equals(c.Chave, selecionada, StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Chave!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var chaves = existentes
            .Where(c => !string.IsNullOrWhiteSpace(c.Chave))
            .Select(c => new ChaveDisponivelViewModel
            {
                Chave = c.Chave!,
                Nome = c.Nome ?? c.Chave!,
                EmUso = emUso.Contains(c.Chave!)
            })
            .DistinctBy(c => c.Chave)
            .ToList();

        foreach (var padrao in new[] { "heritage", "nature", "food", "hotel", "service" })
        {
            if (!chaves.Any(c => c.Chave == padrao))
                chaves.Add(new ChaveDisponivelViewModel
                {
                    Chave = padrao,
                    Nome = "Sugestão do protótipo",
                    EmUso = emUso.Contains(padrao)
                });
        }

        viewData["ChavesDisponiveis"] = chaves;
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
