using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Web.Models;

/// <summary>Base das páginas de seção (textos dinâmicos por chave).</summary>
public abstract class SecaoViewModel
{
    public Dictionary<string, string?> Conteudos { get; set; } = new();
}

/// <summary>Página "Nossa Cidade" — história completa + estatísticas + fotos.</summary>
public class SecaoCidadeViewModel : SecaoViewModel
{
    public IReadOnlyList<EstatisticaDto> Estatisticas { get; set; } = Array.Empty<EstatisticaDto>();
    public IReadOnlyList<SlideDto> Slides { get; set; } = Array.Empty<SlideDto>();
}

/// <summary>Página "Nossa Cultura" — textos + tags culturais.</summary>
public class SecaoCulturaViewModel : SecaoViewModel
{
    public IReadOnlyList<TagCulturalDto> Tags { get; set; } = Array.Empty<TagCulturalDto>();
    public IReadOnlyList<SlideDto> Slides { get; set; } = Array.Empty<SlideDto>();
}

/// <summary>Página "Grupos Populares".</summary>
public class SecaoGruposViewModel : SecaoViewModel
{
    public IReadOnlyList<GrupoCulturalDto> Grupos { get; set; } = Array.Empty<GrupoCulturalDto>();
}

/// <summary>Página "Gastronomia".</summary>
public class SecaoGastronomiaViewModel : SecaoViewModel
{
    public IReadOnlyList<PratoTuristicoDto> Pratos { get; set; } = Array.Empty<PratoTuristicoDto>();
}

/// <summary>Página "Lugares que Encantam" — baralho com as 7 maravilhas.</summary>
public class SecaoLugaresViewModel : SecaoViewModel
{
    public IReadOnlyList<PontoTuristicoDto> Maravilhas { get; set; } = Array.Empty<PontoTuristicoDto>();
    public IReadOnlyList<CategoriaPontoTuristicoDto> Categorias { get; set; } = Array.Empty<CategoriaPontoTuristicoDto>();
}

/// <summary>Detalhe de um ponto turístico (mídias, horários, avaliações, roteiros).</summary>
public class DetalheLugarViewModel
{
    public PontoTuristicoDto Lugar { get; set; } = null!;
    public IReadOnlyList<AvaliacaoDto> Avaliacoes { get; set; } = Array.Empty<AvaliacaoDto>();
    public IReadOnlyList<RoteiroDto> RoteirosComPonto { get; set; } = Array.Empty<RoteiroDto>();
}

