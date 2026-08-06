namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de avaliação de ponto turístico.</summary>
public class AvaliacaoDto
{
    public int Id { get; set; }
    public int PontoTuristicoId { get; set; }
    public string? PontoTuristicoNome { get; set; }
    public string Nome { get; set; } = null!;
    public int Nota { get; set; }
    public string? Comentario { get; set; }
    public DateTime Data { get; set; }
    public bool Aprovada { get; set; }
}
