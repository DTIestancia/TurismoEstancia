using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Avaliacao.Interfaces;
using TurismoEstancia.Services.Planeje.Interfaces;
using TurismoEstancia.Web.Infrastructure;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

/// <summary>
/// Moderação unificada de avaliações: pontos turísticos (mapa) + "Planeje sua
/// viagem". A coluna Origem indica de onde vem cada avaliação; o filtro
/// ?origem=ponto|planeje restringe a lista.
/// </summary>
public class AvaliacoesController : PainelController
{
    private readonly IAvaliacaoService _avaliacoes;
    private readonly IPlanejeService _planeje;

    public AvaliacoesController(IServiceProvider services, IAvaliacaoService avaliacoes, IPlanejeService planeje)
        : base(services)
    {
        _avaliacoes = avaliacoes;
        _planeje = planeje;
    }

    public async Task<IActionResult> Index(CancellationToken ct, string? origem = null, int pagina = 1)
    {
        ViewData["Title"] = "Avaliações";
        origem = origem is "ponto" or "planeje" ? origem : null;

        var pontos = await _avaliacoes.ListarAsync(apenasAprovadas: false, ct);
        var planeje = await _planeje.ListarTodasAvaliacoesAsync(ct);

        var todas = pontos
            .Select(a => new AvaliacaoGerenciadorViewModel
            {
                Origem = "ponto",
                Id = a.Id,
                Local = a.PontoTuristicoNome ?? $"Ponto #{a.PontoTuristicoId}",
                Visitante = string.IsNullOrWhiteSpace(a.Nome) ? "Anônimo" : a.Nome,
                Nota = a.Nota,
                Comentario = a.Comentario,
                Data = a.Data,
                Aprovada = a.Aprovada
            })
            .Concat(planeje.Select(a => new AvaliacaoGerenciadorViewModel
            {
                Origem = "planeje",
                Id = a.Id,
                Local = a.PlanejeItemTitulo ?? $"Local #{a.PlanejeItemId}",
                Visitante = string.IsNullOrWhiteSpace(a.Nome) ? "Anônimo" : a.Nome,
                Nota = a.Nota,
                Comentario = a.Comentario,
                Data = a.Data,
                Aprovada = a.Aprovada
            }))
            .Where(a => origem == null || a.Origem == origem)
            .OrderByDescending(a => a.Data)
            .ToList();

        ViewData["OrigemAtual"] = origem;
        ViewData["TotalPontos"] = pontos.Count;
        ViewData["TotalPlaneje"] = planeje.Count;
        ViewData["PendentesAvaliacoes"] = pontos.Count(a => !a.Aprovada) + planeje.Count(a => !a.Aprovada);

        var totalPaginas = Math.Max(1, (int)Math.Ceiling(todas.Count / (double)PaginaService.TamanhoPainel));
        var paginaAtual = Math.Clamp(pagina, 1, totalPaginas);
        ViewData["PaginaAtual"] = paginaAtual;
        ViewData["PaginasTotal"] = totalPaginas;

        return View(todas
            .Skip((paginaAtual - 1) * PaginaService.TamanhoPainel)
            .Take(PaginaService.TamanhoPainel)
            .ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprovar(int id, string? origem, int pagina, CancellationToken ct)
    {
        try
        {
            await _avaliacoes.AprovarAsync(id, ct);
            TempData["PainelOk"] = "Avaliação aprovada e publicada no portal.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { origem, pagina });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, string? origem, int pagina, CancellationToken ct)
    {
        try
        {
            await _avaliacoes.ExcluirAsync(id, ct);
            TempData["PainelOk"] = "Avaliação removida.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { origem, pagina });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AprovarPlaneje(int id, string? origem, int pagina, CancellationToken ct)
    {
        try
        {
            await _planeje.AprovarAvaliacaoAsync(id, ct);
            TempData["PainelOk"] = "Avaliação aprovada e publicada no portal.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { origem, pagina });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirPlaneje(int id, string? origem, int pagina, CancellationToken ct)
    {
        try
        {
            await _planeje.ExcluirAvaliacaoAsync(id, ct);
            TempData["PainelOk"] = "Avaliação removida.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { origem, pagina });
    }
}
