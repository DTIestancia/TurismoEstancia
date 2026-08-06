using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de grupo cultural.</summary>
public class GrupoCulturalDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [MaxLength(150)]
    public string Nome { get; set; } = null!;
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
