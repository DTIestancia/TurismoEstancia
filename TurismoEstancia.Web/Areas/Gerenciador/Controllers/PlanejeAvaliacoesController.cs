using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Planeje.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class PlanejeAvaliacoesController : PainelController
{
    private readonly IPlanejeService _planeje;

    public PlanejeAvaliacoesController(IServiceProvider services, IPlanejeService planeje)
        : base(services) => _planeje = planeje;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Avaliações do Planeje";
        ViewData["AreaAtiva"] = "roteiros";
        ViewData["Pendentes"] = await _planeje.ListarPendentesAsync(ct);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprovar(int id, CancellationToken ct)
    {
        await _planeje.AprovarAvaliacaoAsync(id, ct);
        TempData["PainelOk"] = "Avaliação aprovada.";
        return RedirecionarParaIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _planeje.ExcluirAvaliacaoAsync(id, ct);
        TempData["PainelOk"] = "Avaliação excluída.";
        return RedirecionarParaIndex();
    }
}
