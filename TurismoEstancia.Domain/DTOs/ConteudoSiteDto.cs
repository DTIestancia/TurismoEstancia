using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de conteúdo do site.</summary>
public class ConteudoSiteDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe a chave (ex.: hero-titulo).")]
    [MaxLength(60)]
    public string Chave { get; set; } = null!;

    [Required(ErrorMessage = "Informe o nome.")]
    [MaxLength(120)]
    public string Nome { get; set; } = null!;
    public string? Texto { get; set; }
}
