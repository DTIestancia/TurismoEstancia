using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de roteiro turístico.</summary>
public class RoteiroDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o título.")]
    [MaxLength(180)]
    public string Titulo { get; set; } = null!;
    public string? Descricao { get; set; }
    public long? ImagemArquivoId { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;

    public List<RoteiroItemDto> Itens { get; set; } = new();
}
