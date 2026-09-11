using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de avaliação de item do "Planeje sua viagem". Nome opcional.</summary>
public class PlanejeAvaliacaoDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Item inválido.")]
    public int PlanejeItemId { get; set; }

    public string? PlanejeItemTitulo { get; set; }

    [MaxLength(150)]
    public string? Nome { get; set; }

    [Range(1, 5, ErrorMessage = "A nota deve estar entre 1 e 5.")]
    public int Nota { get; set; } = 5;

    [Required(ErrorMessage = "Conte sua experiência (mínimo 50 caracteres).")]
    [MinLength(50, ErrorMessage = "Conte sua experiência com ao menos 50 caracteres.")]
    [MaxLength(1000)]
    public string Comentario { get; set; } = null!;

    public DateTime Data { get; set; }
    public bool Aprovada { get; set; }
}
