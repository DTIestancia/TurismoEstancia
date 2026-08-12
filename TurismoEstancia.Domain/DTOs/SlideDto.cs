namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de slide do hero.</summary>
public class SlideDto
{
    public int Id { get; set; }
    public long ImagemArquivoId { get; set; }
    public string? Titulo { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
