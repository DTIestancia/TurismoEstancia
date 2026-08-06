using System.ComponentModel.DataAnnotations;
using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de configuração do site.</summary>
public class ConfiguracaoSiteDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe a chave (ex.: guia-pdf).")]
    [MaxLength(60)]
    public string Chave { get; set; } = null!;

    [Required(ErrorMessage = "Informe o nome.")]
    [MaxLength(120)]
    public string Nome { get; set; } = null!;
    public TipoConfiguracao Tipo { get; set; }
    public string? ValorTexto { get; set; }
    public long? ArquivoId { get; set; }
    public string? ArquivoNome { get; set; }
}
