namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Foto da Galeria de Estância. Os bytes ficam na tabela <see cref="Arquivo"/>
/// (nunca no disco): <see cref="ArquivoId"/> é a imagem otimizada (máx. 1600px,
/// usada no lightbox) e <see cref="ArquivoThumbId"/> o thumbnail (400px) usado
/// nos grids — a otimização no upload evita pesar o banco.
/// </summary>
public class GaleriaMidia
{
    public int Id { get; set; }

    public int CategoriaId { get; set; }

    public GaleriaCategoria? Categoria { get; set; }

    /// <summary>Imagem otimizada (JPEG, máx. 1600px) — FK Restrict para Arquivo.</summary>
    public long ArquivoId { get; set; }

    public Arquivo? Arquivo { get; set; }

    /// <summary>Thumbnail (JPEG, 400px) para os grids — FK Restrict para Arquivo.</summary>
    public long? ArquivoThumbId { get; set; }

    public Arquivo? Thumb { get; set; }

    /// <summary>Legenda da foto (acessibilidade/SEO/lightbox).</summary>
    public string? Titulo { get; set; }

    /// <summary>Ordenação manual dentro da categoria.</summary>
    public int Ordem { get; set; }

    /// <summary>Oculta a foto no portal sem apagar o binário.</summary>
    public bool Ativo { get; set; } = true;

    /// <summary>Total de visualizações (lightbox aberto). Incrementado por endpoint do portal.</summary>
    public int Visualizacoes { get; set; }

    /// <summary>Total de curtidas ("Amei"). Uma sessão só curte uma vez (dedup anônimo).</summary>
    public int Curtidas { get; set; }
}
