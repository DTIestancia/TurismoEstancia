using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de item do Mídia kit.</summary>
public class MidiaKitItemDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o título.")]
    [MaxLength(180)]
    public string Titulo { get; set; } = null!;

    [MaxLength(1000)]
    public string? Descricao { get; set; }

    public long? ArquivoId { get; set; }

    public string? ArquivoNome { get; set; }

    public string? ArquivoContentType { get; set; }

    public long? ArquivoSize { get; set; }

    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime Data { get; set; }
}
