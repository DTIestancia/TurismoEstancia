using System.ComponentModel.DataAnnotations;
using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de evento da agenda.</summary>
public class EventoDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o título.")]
    [MaxLength(150)]
    public string Titulo { get; set; } = null!;
    public string? Descricao { get; set; }
    public string? Local { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;

    /// <summary>True quando o evento ainda não terminou (exibido no portal).</summary>
    public bool EProximo => DataFim >= DateTime.Today;
}
