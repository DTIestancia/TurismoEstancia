using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Avaliacao.Interfaces;
using TurismoEstancia.Web.Infrastructure;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class AvaliacoesController : PainelController
{
    private readonly IAvaliacaoService _avaliacoes;

    public AvaliacoesController(IServiceProvider services, IAvaliacaoService avaliacoes)
        : base(services) => _avaliacoes = avaliacoes;

    public async Task<IActionResult> Index(CancellationToken ct, int pagina = 1)
    {
        ViewData["Title"] = "Avaliações";
        var todas = await _avaliacoes.ListarAsync(apenasAprovadas: false, ct);
        ViewData["PendentesAvaliacoes"] = todas.Count(a => !a.Aprovada);

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
    public async Task<IActionResult> Aprovar(int id, CancellationToken ct)
    {
        await _avaliacoes.AprovarAsync(id, ct);
        TempData["PainelOk"] = "Avaliação aprovada e publicada no portal.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _avaliacoes.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Avaliação removida.";
        return RedirectToAction(nameof(Index));
    }
}
