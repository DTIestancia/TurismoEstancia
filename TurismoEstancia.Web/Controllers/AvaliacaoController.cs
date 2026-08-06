using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Avaliacao.Interfaces;

namespace TurismoEstancia.Web.Controllers;

/// <summary>Submissão de avaliações no portal (entram para moderação no CMS).</summary>
public class AvaliacaoController : Controller
{
    private readonly IAvaliacaoService _avaliacoes;

    public AvaliacaoController(IAvaliacaoService avaliacoes) => _avaliacoes = avaliacoes;

    /// <summary>
    /// POST /Avaliacao/Submeter — nome, nota 1-5 e comentário.
    /// <paramref name="retorno"/> (opcional) devolve o usuário à página de
    /// origem (ex.: detalhe do ponto) em vez da home. Só aceita caminhos
    /// relativos do próprio site (anti open-redirect).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submeter(AvaliacaoDto dto, string? retorno = null)
    {
        var ct = HttpContext.RequestAborted;

        // Url.IsLocalUrl rejeita caminhos externos e bypasos (ex.: /\\host,
        // //host, http://...), permitindo só caminhos do próprio site.
        var destino = !string.IsNullOrWhiteSpace(retorno) && Url.IsLocalUrl(retorno)
            ? retorno
            : Url.Action(nameof(HomeController.Index), "Home") ?? "/";

        try
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                ModelState.AddModelError("Nome", "Informe seu nome.");

            if (dto.Nota < 1 || dto.Nota > 5)
                ModelState.AddModelError("Nota", "A nota deve estar entre 1 e 5.");

            if (ModelState.ErrorCount > 0)
            {
                TempData["AvaliacaoErro"] = "Verifique os dados da avaliação.";
                return Redirect(destino);
            }

            await _avaliacoes.SubmeterAsync(dto, ct);
            TempData["AvaliacaoOk"] = "Obrigado! Sua avaliação foi enviada e aguarda moderação.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["AvaliacaoErro"] = ex.Message;
        }

        return Redirect(destino);
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
