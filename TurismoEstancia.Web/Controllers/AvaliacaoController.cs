using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Avaliacao.Interfaces;

namespace TurismoEstancia.Web.Controllers;

/// <summary>Submissão de avaliações no portal (entram para moderação no CMS).</summary>
public class AvaliacaoController : Controller
{
    private readonly IAvaliacaoService _avaliacoes;

    public AvaliacaoController(IAvaliacaoService avaliacoes) => _avaliacoes = avaliacoes;

    /// <summary>POST /Avaliacao/Submeter — nome, nota 1-5 e comentário.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submeter(AvaliacaoDto dto, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                ModelState.AddModelError("Nome", "Informe seu nome.");

            if (dto.Nota < 1 || dto.Nota > 5)
                ModelState.AddModelError("Nota", "A nota deve estar entre 1 e 5.");

            if (ModelState.ErrorCount > 0)
            {
                TempData["AvaliacaoErro"] = "Verifique os dados da avaliação.";
                return RedirectToAction(nameof(HomeController.Index), "Home", null);
            }

            await _avaliacoes.SubmeterAsync(dto, ct);
            TempData["AvaliacaoOk"] = "Obrigado! Sua avaliação foi enviada e aguarda moderação.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["AvaliacaoErro"] = ex.Message;
        }

        return RedirectToAction(nameof(HomeController.Index), "Home", null);
    }

    /// <summary>GET /Avaliacao/ListarPorPonto/{id} — JSON das avaliações aprovadas (modal do mapa).</summary>
    [HttpGet]
    [Route("Avaliacao/ListarPorPonto/{id:int}")]
    public async Task<IActionResult> ListarPorPonto(int id, CancellationToken ct)
    {
        var avaliacoes = await _avaliacoes.ListarPorPontoAsync(id, apenasAprovadas: true, ct);
        return Json(avaliacoes);
    }
}
