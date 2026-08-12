namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Notícia do portal (/Noticias). Publicada controla a exibição;
/// Slug único compõe a URL amigável.
/// </summary>
public class Noticia
{
    public int Id { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Resumo { get; set; }

    public string? Corpo { get; set; }

    public long? ImagemArquivoId { get; set; }

    public Arquivo? Imagem { get; set; }

    /// <summary>Zoom (%) aplicado ao recorte da imagem de capa (100 = sem zoom, até 250).</summary>
    public int ImagemZoom { get; set; } = 100;

    /// <summary>Posição horizontal do foco do recorte (object-position X, 0–100).</summary>
    public int ImagemPosicaoX { get; set; } = 50;

    /// <summary>Posição vertical do foco do recorte (object-position Y, 0–100).</summary>
    public int ImagemPosicaoY { get; set; } = 50;

    /// <summary>Galeria (categoria da Galeria de Estância) relacionada à notícia — exibida na página de detalhe.</summary>
    public int? GaleriaCategoriaId { get; set; }

    public GaleriaCategoria? Galeria { get; set; }

    public DateTime DataPublicacao { get; set; }

    /// <summary>Slug único para a URL amigável (ex.: "barco-de-fogo-2026").</summary>
    public string Slug { get; set; } = null!;

    /// <summary>Indica se a notícia está visível no portal.</summary>
    public bool Publicada { get; set; } = false;

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;
}
