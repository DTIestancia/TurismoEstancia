using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Avaliacao.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class AvaliacoesController : PainelController
{
    private readonly IAvaliacaoService _avaliacoes;

    public AvaliacoesController(IServiceProvider services, IAvaliacaoService avaliacoes)
        : base(services) => _avaliacoes = avaliacoes;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Avaliações";
        var todas = await _avaliacoes.ListarAsync(apenasAprovadas: false, ct);
        ViewData["PendentesAvaliacoes"] = todas.Count(a => !a.Aprovada);
        return View(todas);
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
