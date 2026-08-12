using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Web.Models;

/// <summary>Página pública da Galeria de Estância (/galeria e /galeria/{chave}).</summary>
public class GaleriaViewModel
{
    /// <summary>Categorias ativas (pílulas de filtro).</summary>
    public IReadOnlyList<GaleriaCategoriaDto> Categorias { get; set; } = Array.Empty<GaleriaCategoriaDto>();

    /// <summary>Categoria selecionada (null = "Todas").</summary>
    public GaleriaCategoriaDto? CategoriaAtual { get; set; }

    /// <summary>Fotos exibidas no grid/lightbox (com thumbnail + imagem cheia).</summary>
    public IReadOnlyList<GaleriaMidiaDto> Fotos { get; set; } = Array.Empty<GaleriaMidiaDto>();

    /// <summary>Total real de fotos (para o contador, mesmo quando a exibição é limitada).</summary>
    public int FotosTotal { get; set; }
}
