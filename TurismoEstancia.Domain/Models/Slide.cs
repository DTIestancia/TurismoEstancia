namespace TurismoEstancia.Domain.Models;

/// <summary>Slide do carrossel do hero (imagem + legenda).</summary>
public class Slide
{
    public int Id { get; set; }

    public long ImagemArquivoId { get; set; }

    public Arquivo? Imagem { get; set; }

    /// <summary>Texto alternativo (acessibilidade/SEO).</summary>
    public string? Titulo { get; set; }

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;
}
