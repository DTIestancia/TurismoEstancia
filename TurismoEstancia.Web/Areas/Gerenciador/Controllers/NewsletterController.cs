using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TurismoEstancia.Mail;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class NewsletterController : PainelController
{
    private readonly IInscricaoNewsletterService _newsletter;
    private readonly IEmailQueue _emailQueue;
    private readonly SmtpConfig _smtp;

    public NewsletterController(
        IServiceProvider services,
        IInscricaoNewsletterService newsletter,
        IEmailQueue emailQueue,
        IOptions<SmtpConfig> smtp)
        : base(services)
    {
        _newsletter = newsletter;
        _emailQueue = emailQueue;
        _smtp = smtp.Value;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Newsletter";
        var inscricoes = await _newsletter.ListarAsync(incluirInativos: true, ct);
        var destinatarios = await _newsletter.ListarEmailsAtivosAsync(ct);
        return View(new NewsletterIndexViewModel
        {
            Inscricoes = inscricoes,
            Destinatarios = destinatarios.Count,
            SmtpConfigurado = _smtp.Configurado
        });
    }

    /// <summary>Exporta as inscrições ativas em CSV (BOM UTF-8, abre no Excel).</summary>
    public async Task<IActionResult> ExportarCsv(CancellationToken ct)
    {
        var bytes = await _newsletter.ExportarCsvAsync(ct);
        return File(bytes, "text/csv; charset=utf-8", $"newsletter-{DateTime.Now:yyyyMMdd-HHmm}.csv");
    }

    /// <summary>Disparo em massa: enfileira um e-mail para cada destinatário ativo.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarDisparo(DisparoNewsletterViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["PainelErro"] = string.Join(" ", ModelState.Values
                .SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index));
        }

        if (!_smtp.Configurado)
        {
            TempData["PainelErro"] =
                "E-mail SMTP não configurado. Adicione a seção \"Smtp\" (Host e RemetenteEmail) no appsettings.json.";
            return RedirectToAction(nameof(Index));
        }

        var destinatarios = await _newsletter.ListarEmailsAtivosAsync(ct);
        if (destinatarios.Count == 0)
        {
            TempData["PainelErro"] = "Nenhuma inscrição ativa para receber o disparo.";
            return RedirectToAction(nameof(Index));
        }

        var corpoHtml = EmailHtml.Marketing(model.Assunto, model.Corpo);

        // Fila bounded (5000): se encher, para de enfileirar e reporta o parcial
        // em vez de derrubar o request com 500.
        var enfileirados = 0;
        foreach (var email in destinatarios)
        {
            try
            {
                _emailQueue.Enqueue(new EmailJob(email, model.Assunto.Trim(), corpoHtml));
                enfileirados++;
            }
            catch (InvalidOperationException)
            {
                break;
            }
        }

        TempData["PainelOk"] = enfileirados == destinatarios.Count
            ? $"Disparo enfileirado para {enfileirados} destinatário(s): \"{model.Assunto.Trim()}\"."
            : $"Disparo parcial: {enfileirados} de {destinatarios.Count} enfileirados (fila de e-mails cheia).";
        return RedirectToAction(nameof(Index));
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
