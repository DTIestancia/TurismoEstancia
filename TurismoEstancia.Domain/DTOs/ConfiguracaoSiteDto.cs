using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de configuração do site.</summary>
public class ConfiguracaoSiteDto
{
    public int Id { get; set; }
    public string Chave { get; set; } = null!;
    public string Nome { get; set; } = null!;
    public TipoConfiguracao Tipo { get; set; }
    public string? ValorTexto { get; set; }
    public long? ArquivoId { get; set; }
    public string? ArquivoNome { get; set; }
}
