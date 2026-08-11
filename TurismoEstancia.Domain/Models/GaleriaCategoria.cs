namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Categoria dinâmica da Galeria de Estância (ex.: Praias, Patrimônio,
/// Festas e Tradições). As categorias são cadastradas no CMS sem tocar nas
/// fotos; a chave (slug) alimenta a URL pública /galeria/{chave}.
/// </summary>
public class GaleriaCategoria
{
    public int Id { get; set; }

    /// <summary>Nome exibido no portal (ex.: "Praias").</summary>
    public string Nome { get; set; } = null!;

    /// <summary>Chave/slug única usada na URL pública (/galeria/{chave}).</summary>
    public string Chave { get; set; } = null!;

    /// <summary>Legenda curta exibida na página da categoria.</summary>
    public string? Descricao { get; set; }

    /// <summary>Imagem de capa da categoria (otimizada) — usada no card da galeria e no OG/SEO.</summary>
    public long? CapaArquivoId { get; set; }

    public Arquivo? Capa { get; set; }

    /// <summary>Ordenação das categorias no portal.</summary>
    public int Ordem { get; set; }

    /// <summary>Soft-delete.</summary>
    public bool Ativo { get; set; } = true;

    public List<GaleriaMidia> Midias { get; set; } = new();
}
