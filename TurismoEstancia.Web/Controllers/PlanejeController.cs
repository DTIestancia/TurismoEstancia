using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Planeje.Interfaces;

namespace TurismoEstancia.Web.Controllers;

/// <summary>Avaliações dos cards do "Planeje sua viagem" (entram para moderação).</summary>
public class PlanejeController : Controller
{
    private readonly IPlanejeService _planeje;

    public PlanejeController(IPlanejeService planeje) => _planeje = planeje;

    /// <summary>GET /Planeje/Avaliacoes/{id} — JSON das avaliações aprovadas.</summary>
    [HttpGet]
    [Route("Planeje/Avaliacoes/{id:int}")]
    public async Task<IActionResult> Avaliacoes(int id, CancellationToken ct)
    {
        var avaliacoes = await _planeje.ListarAvaliacoesAsync(id, apenasAprovadas: true, ct);
        return Json(avaliacoes);
    }

    /// <summary>
    /// POST /Planeje/Avaliar — nome opcional, nota 1-5 e comentário (mín. 50).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Avaliar(PlanejeAvaliacaoDto dto, string? retorno = null)
    {
        var destino = !string.IsNullOrWhiteSpace(retorno) && Url.IsLocalUrl(retorno)
            ? retorno
            : Url.Action(nameof(HomeController.Index), "Home") ?? "/";

        try
        {
            await _planeje.SubmeterAvaliacaoAsync(dto, HttpContext.RequestAborted);
            TempData["AvaliacaoOk"] = "Obrigado! Sua avaliação foi enviada e aguarda moderação.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["AvaliacaoErro"] = ex.Message;
        }

        return Redirect(destino);
    }
}
