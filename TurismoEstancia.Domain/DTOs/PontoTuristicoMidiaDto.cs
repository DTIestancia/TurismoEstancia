using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de mídia de ponto turístico.</summary>
public class PontoTuristicoMidiaDto
{
    public int Id { get; set; }
    public int PontoTuristicoId { get; set; }
    public long ArquivoId { get; set; }
    public string? ArquivoNome { get; set; }
    public TipoMidia Tipo { get; set; }
    public int Ordem { get; set; }
}
