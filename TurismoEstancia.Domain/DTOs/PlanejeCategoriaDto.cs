using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de categoria do "Planeje sua viagem".</summary>
public class PlanejeCategoriaDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [MaxLength(100)]
    public string Nome { get; set; } = null!;

    [MaxLength(500)]
    public string? Descricao { get; set; }

    [MaxLength(50)]
    public string? Icone { get; set; }

    [MaxLength(20)]
    public string? Cor { get; set; }

    public long? ImagemPadraoArquivoId { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
