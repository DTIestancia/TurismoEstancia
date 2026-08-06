namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Arquivo binário (imagem, vídeo, PDF) persistido em byte[] no banco,
/// servido pelo endpoint <c>GET /arquivo/{id}</c>. Nunca salvo no disco.
/// </summary>
public class Arquivo
{
    public long Id { get; set; }

    /// <summary>Nome original do arquivo.</summary>
    public string Nome { get; set; } = null!;

    /// <summary>Content-Type (ex.: image/jpeg, video/mp4, application/pdf).</summary>
    public string ContentType { get; set; } = null!;

    public byte[] Bytes { get; set; } = null!;

    public DateTime CriadoEm { get; set; }
}
