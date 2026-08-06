using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de estatística.</summary>
public class EstatisticaDto
{
    public int Id { get; set; }
    public string Valor { get; set; } = null!;
    public string? Legenda { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
