using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Comunicacao.Interfaces;

namespace TurismoEstancia.Web.Controllers;

/// <summary>Inscrição na newsletter (portal público).</summary>
public class NewsletterController : Controller
{
    private readonly IInscricaoNewsletterService _newsletter;

    public NewsletterController(IInscricaoNewsletterService newsletter) => _newsletter = newsletter;

    /// <summary>POST /Newsletter/Inscrever — salva o e-mail com consentimento LGPD.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscrever(string email, bool consentimentoLgpd, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) ||
                !Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                TempData["NewsletterErro"] = "Informe um e-mail válido.";
                return RedirectToAction(nameof(HomeController.Index), "Home", null);
            }

            await _newsletter.InscreverAsync(email, "rodape", consentimentoLgpd, ct);
            TempData["NewsletterOk"] = "Inscrição realizada com sucesso!";
        }
        catch (InvalidOperationException ex)
        {
            TempData["NewsletterErro"] = ex.Message;
        }

        return RedirectToAction(nameof(HomeController.Index), "Home", null);
    }
}
