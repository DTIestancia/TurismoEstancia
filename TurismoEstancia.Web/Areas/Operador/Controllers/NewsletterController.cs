using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Comunicacao.Interfaces;

namespace TurismoEstancia.Web.Areas.Operador.Controllers;

public class NewsletterController : OperadorController
{
    private readonly IInscricaoNewsletterService _newsletter;

    public NewsletterController(IServiceProvider services, IInscricaoNewsletterService newsletter)
        : base(services) => _newsletter = newsletter;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Newsletter";
        return View(await _newsletter.ListarAsync(incluirInativos: true, ct));
    }

    public async Task<IActionResult> ExportarCsv(CancellationToken ct)
    {
        var bytes = await _newsletter.ExportarCsvAsync(ct);
        return File(bytes, "text/csv; charset=utf-8", $"newsletter-{DateTime.Now:yyyyMMdd-HHmm}.csv");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inativar(int id, CancellationToken ct)
    {
        await _newsletter.InativarAsync(id, ct);
        TempData["PainelOk"] = "Inscrição inativada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reativar(int id, CancellationToken ct)
    {
        await _newsletter.ReativarAsync(id, ct);
        TempData["PainelOk"] = "Inscrição reativada.";
        return RedirectToAction(nameof(Index));
    }
}
