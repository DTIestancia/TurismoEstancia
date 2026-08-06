namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de prato turístico.</summary>
public class PratoTuristicoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
