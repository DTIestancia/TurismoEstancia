namespace TurismoEstancia.Mail;

/// <summary>
/// Configuração SMTP (seção "Smtp" do appsettings). Sem Host configurado
/// o envio fica desativado — o EmailSender lança uma mensagem clara.
/// </summary>
public class SmtpConfig
{
    public string? Host { get; set; }
    public int Porta { get; set; } = 587;
    public string? Usuario { get; set; }
    public string? Senha { get; set; }
    public string? RemetenteEmail { get; set; }
    public string? RemetenteNome { get; set; } = "Descubra Estância";
    public bool UsarSsl { get; set; } = true;

    /// <summary>True quando há Host e remetente — sem isso o envio é bloqueado.</summary>
    public bool Configurado =>
        !string.IsNullOrWhiteSpace(Host) &&
        !string.IsNullOrWhiteSpace(RemetenteEmail);
}
