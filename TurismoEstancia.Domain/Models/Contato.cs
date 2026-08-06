namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Contato do rodapé: endereço, telefones (190, SAMU, SMTT, CIT) e redes sociais.
/// Cada item só aparece se preenchido.
/// </summary>
public class Contato
{
    public int Id { get; set; }

    public TipoContato Tipo { get; set; }

    /// <summary>Rótulo exibido (ex.: "SAMU 192", "Instagram").</summary>
    public string? Rotulo { get; set; }

    /// <summary>Valor: texto do endereço, número do telefone ou URL da rede.</summary>
    public string Valor { get; set; } = null!;

    /// <summary>Nome do ícone lucide exibido ao lado do contato.</summary>
    public string? Icone { get; set; }

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;
}
