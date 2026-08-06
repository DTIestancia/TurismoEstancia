namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Item de um roteiro: associa um ponto turístico a um dia do roteiro
/// (Dia ≥ 1) com observação opcional.
/// </summary>
public class RoteiroItem
{
    public int Id { get; set; }

    public int RoteiroId { get; set; }

    public Roteiro? Roteiro { get; set; }

    public int PontoTuristicoId { get; set; }

    public PontoTuristico? PontoTuristico { get; set; }

    /// <summary>Dia do roteiro (1 = primeiro dia).</summary>
    public int Dia { get; set; }

    public int Ordem { get; set; }

    public string? Observacao { get; set; }
}
