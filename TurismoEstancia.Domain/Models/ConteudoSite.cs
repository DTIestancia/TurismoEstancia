namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Bloco de texto de uma seção do portal, identificado por uma chave única
/// (ex.: "hero-titulo", "historia-texto", "newsletter-titulo").
/// </summary>
public class ConteudoSite
{
    public int Id { get; set; }

    /// <summary>Chave única usada no código para localizar o conteúdo.</summary>
    public string Chave { get; set; } = null!;

    /// <summary>Nome descritivo exibido no CMS.</summary>
    public string Nome { get; set; } = null!;

    public string? Texto { get; set; }
}
