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

    /// <summary>Total real de fotos (para o contador e a paginação).</summary>
    public int FotosTotal { get; set; }

    /// <summary>Página atual do grid (a partir de ?pagina=).</summary>
    public int PaginaAtual { get; set; } = 1;

    /// <summary>Total de páginas (FotosTotal / fotos por página).</summary>
    public int PaginasTotal { get; set; } = 1;

    /// <summary>Fotos por página (padrão 12).</summary>
    public int TamanhoPagina { get; set; } = 12;
}
