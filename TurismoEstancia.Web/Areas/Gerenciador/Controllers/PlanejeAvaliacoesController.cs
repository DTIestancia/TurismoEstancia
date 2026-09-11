using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Planeje.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

/// <summary>
/// Compatibilidade: a moderação do Planeje foi unificada em
/// <see cref="AvaliacoesController"/> (filtro ?origem=planeje). As ações de
/// aprovar/excluir seguem funcionando e voltam para a tela unificada.
/// </summary>
public class PlanejeAvaliacoesController : PainelController
{
    private readonly IPlanejeService _planeje;

    public PlanejeAvaliacoesController(IServiceProvider services, IPlanejeService planeje)
        : base(services) => _planeje = planeje;

    public IActionResult Index() =>
        RedirectToAction("Index", "Avaliacoes", new { area = "Gerenciador", origem = "planeje" });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprovar(int id, CancellationToken ct)
    {
        try
        {
            await _planeje.AprovarAvaliacaoAsync(id, ct);
            TempData["PainelOk"] = "Avaliação aprovada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction("Index", "Avaliacoes", new { area = "Gerenciador", origem = "planeje" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        try
        {
            await _planeje.ExcluirAvaliacaoAsync(id, ct);
            TempData["PainelOk"] = "Avaliação excluída.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction("Index", "Avaliacoes", new { area = "Gerenciador", origem = "planeje" });
    }
}
