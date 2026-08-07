namespace TurismoEstancia.Mail;

/// <summary>Envio de e-mail via SMTP (MailKit). Lança InvalidOperationException
/// quando o SMTP não está configurado ou o envio falha.</summary>
public interface IEmailSender
{
    Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct = default);
}
