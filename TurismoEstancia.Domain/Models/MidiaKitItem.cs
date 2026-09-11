namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Item do Mídia kit (/midia-kit): material para imprensa e parceiros
/// (logotipos, fotos em alta, manual de marca...). O binário fica na tabela
/// <see cref="Arquivo"/> (nunca no disco); o download usa <c>GET /arquivo/{id}</c>.
/// </summary>
public class MidiaKitItem
{
    public int Id { get; set; }

    /// <summary>Título exibido no card (ex.: "Logotipo oficial (PNG)").</summary>
    public string Titulo { get; set; } = null!;

    /// <summary>Descrição curta (formato, uso recomendado...).</summary>
    public string? Descricao { get; set; }

    /// <summary>Arquivo para download (qualquer tipo: PNG, PDF, ZIP...).</summary>
    public long? ArquivoId { get; set; }

    public Arquivo? Arquivo { get; set; }

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;

    /// <summary>Data de cadastro (GETDATE()).</summary>
    public DateTime Data { get; set; }
}
