namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Inscrição na newsletter. E-mail único — reenvio reativa a inscrição
/// (Ativo = true) em vez de duplicar. Exclusão = Ativo = false.
/// </summary>
public class InscricaoNewsletter
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    /// <summary>Origem da inscrição (ex.: "rodape", "portal").</summary>
    public string? Origem { get; set; }

    /// <summary>Consentimento LGPD obrigatório no formulário.</summary>
    public bool ConsentimentoLgpd { get; set; }

    public DateTime DataInscricao { get; set; }

    public bool Ativo { get; set; } = true;
}
