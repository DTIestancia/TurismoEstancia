using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace TurismoEstancia.Mail;

/// <summary>Envio de e-mail via SMTP (MailKit), lendo a seção "Smtp" da config.</summary>
public class EmailSender : IEmailSender
{
    private readonly SmtpConfig _config;

    public EmailSender(IOptions<SmtpConfig> config) => _config = config.Value;

    public async Task EnviarAsync(string para, string assunto, string corpoHtml, CancellationToken ct = default)
    {
        if (!_config.Configurado)
            throw new InvalidOperationException(
                "E-mail SMTP não configurado. Adicione a seção \"Smtp\" no appsettings.json.");

        var mensagem = new MimeMessage();
        mensagem.From.Add(new MailboxAddress(_config.RemetenteNome, _config.RemetenteEmail!));
        mensagem.To.Add(MailboxAddress.Parse(para));
        mensagem.Subject = assunto;
        mensagem.Body = new BodyBuilder { HtmlBody = corpoHtml }.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_config.Host!, _config.Porta,
                _config.UsarSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, ct);

            if (!string.IsNullOrWhiteSpace(_config.Usuario))
                await client.AuthenticateAsync(_config.Usuario, _config.Senha ?? string.Empty, ct);

            await client.SendAsync(mensagem, ct);
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, ct);
        }
    }
}
