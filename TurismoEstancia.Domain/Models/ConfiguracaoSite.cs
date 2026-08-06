namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Configuração única do site (chave única). Pode ser um texto
/// (título do site, meta descrição) ou um arquivo (guia PDF, vídeo institucional).
/// </summary>
public class ConfiguracaoSite
{
    public int Id { get; set; }

    /// <summary>Chave única usada no código (ex.: "guia-pdf", "video-institucional", "site-titulo").</summary>
    public string Chave { get; set; } = null!;

    public string Nome { get; set; } = null!;

    public TipoConfiguracao Tipo { get; set; }

    public string? ValorTexto { get; set; }

    public long? ArquivoId { get; set; }

    public Arquivo? Arquivo { get; set; }
}
