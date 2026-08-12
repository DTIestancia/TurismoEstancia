using System.ComponentModel.DataAnnotations;
using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Web.Models;

/// <summary>Página de gerenciamento da newsletter: lista + contadores.</summary>
public class NewsletterIndexViewModel
{
    /// <summary>Inscrições da página atual (paginadas).</summary>
    public IReadOnlyList<InscricaoNewsletterDto> Inscricoes { get; set; } = Array.Empty<InscricaoNewsletterDto>();

    /// <summary>Totais globais (calculados sobre a lista completa, não só a página).</summary>
    public int Total { get; set; }

    public int Ativas { get; set; }

    public int Inativas { get; set; }

    /// <summary>Destinatários do disparo (ativas com consentimento LGPD).</summary>
    public int Destinatarios { get; set; }

    /// <summary>True quando há SMTP configurado no appsettings (senão o envio é bloqueado).</summary>
    public bool SmtpConfigurado { get; set; }
}

/// <summary>Formulário do disparo em massa (assunto + corpo em texto puro).</summary>
public class DisparoNewsletterViewModel
{
    [Required(ErrorMessage = "Informe o assunto do e-mail.")]
    [StringLength(120, ErrorMessage = "O assunto deve ter no máximo 120 caracteres.")]
    [Display(Name = "Assunto")]
    public string Assunto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escreva a mensagem do e-mail.")]
    [StringLength(5000, ErrorMessage = "A mensagem deve ter no máximo 5000 caracteres.")]
    [Display(Name = "Mensagem")]
    public string Corpo { get; set; } = string.Empty;
}
