namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de conteúdo do site.</summary>
public class ConteudoSiteDto
{
    public int Id { get; set; }
    public string Chave { get; set; } = null!;
    public string Nome { get; set; } = null!;
    public string? Texto { get; set; }
}
