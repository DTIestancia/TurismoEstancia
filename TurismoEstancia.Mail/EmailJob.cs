namespace TurismoEstancia.Mail;

/// <summary>Um e-mail aguardando envio na fila.</summary>
public sealed record EmailJob(string Para, string Assunto, string CorpoHtml);
