using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de item de roteiro.</summary>
public class RoteiroItemDto
{
    public int Id { get; set; }
    public int RoteiroId { get; set; }
    public int PontoTuristicoId { get; set; }
    public string? PontoTuristicoNome { get; set; }
    public int Dia { get; set; }
    public int Ordem { get; set; }
    public string? Observacao { get; set; }
}
