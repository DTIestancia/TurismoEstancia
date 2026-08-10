using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Turismo.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class EstatisticasController : PainelController
{
    private readonly IEstatisticaService _estatisticas;

    public EstatisticasController(IServiceProvider services, IEstatisticaService estatisticas)
        : base(services) => _estatisticas = estatisticas;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Estatísticas";
        ViewData["AreaAtiva"] = "cidade";
        return View(await _estatisticas.ListarAsync(ct));
    }

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["Title"] = "Nova estatística";
        ViewData["AreaAtiva"] = "cidade";
        await PreencherLegendasAsync(ViewData, ct);
        return View(new EstatisticaDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(EstatisticaDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PreencherLegendasAsync(ViewData, ct);
            return View(dto);
        }
        await _estatisticas.SalvarAsync(dto, ct);
        TempData["PainelOk"] = "Estatística salva.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar estatística";
        ViewData["AreaAtiva"] = "cidade";
        var dto = await _estatisticas.ObterPorIdAsync(id, ct);
        if (dto is null) return NotFound();
        await PreencherLegendasAsync(ViewData, ct, dto.Legenda);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EstatisticaDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PreencherLegendasAsync(ViewData, ct, dto.Legenda);
            return View(dto);
        }
        await _estatisticas.SalvarAsync(dto, ct);
        TempData["PainelOk"] = "Estatística atualizada.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Legendas comuns da seção de história + as já usadas por outras
    /// estatísticas, para o Gerenciador escolher em vez de digitar.
    /// </summary>
    private async Task PreencherLegendasAsync(ViewDataDictionary viewData, CancellationToken ct, string? selecionada = null)
    {
        var emUso = (await _estatisticas.ListarAsync(ct))
            .Where(e => !string.IsNullOrWhiteSpace(e.Legenda) && !string.Equals(e.Legenda, selecionada, StringComparison.OrdinalIgnoreCase))
            .Select(e => e.Legenda!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        viewData["LegendasDisponiveis"] = new List<ChaveDisponivelViewModel>
        {
            new() { Chave = "anos", Nome = "anos", EmUso = emUso.Contains("anos") },
            new() { Chave = "anos de história", Nome = "anos de história", EmUso = emUso.Contains("anos de história") },
            new() { Chave = "habitantes", Nome = "habitantes", EmUso = emUso.Contains("habitantes") },
            new() { Chave = "maravilhas", Nome = "maravilhas", EmUso = emUso.Contains("maravilhas") },
            new() { Chave = "Km de praias", Nome = "Km de praias", EmUso = emUso.Contains("Km de praias") },
            new() { Chave = "eventos por ano", Nome = "eventos por ano", EmUso = emUso.Contains("eventos por ano") },
            new() { Chave = "bares e restaurantes", Nome = "bares e restaurantes", EmUso = emUso.Contains("bares e restaurantes") },
            new() { Chave = "filarmônicas", Nome = "filarmônicas", EmUso = emUso.Contains("filarmônicas") }
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _estatisticas.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Estatística excluída.";
        return RedirectToAction(nameof(Index));
    }
}
