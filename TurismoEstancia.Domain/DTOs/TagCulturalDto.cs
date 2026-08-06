using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de tag cultural.</summary>
public class TagCulturalDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [MaxLength(80)]
    public string Nome { get; set; } = null!;
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
