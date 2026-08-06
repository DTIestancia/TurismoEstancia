namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de inscrição na newsletter.</summary>
public class InscricaoNewsletterDto
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string? Origem { get; set; }
    public bool ConsentimentoLgpd { get; set; }
    public DateTime DataInscricao { get; set; }
    public bool Ativo { get; set; } = true;
}
