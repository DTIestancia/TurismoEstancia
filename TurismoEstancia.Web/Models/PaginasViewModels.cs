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

/// <summary>Mostruário "Nossa Cidade" — fotos, texto e carrossel de postais.</summary>
public class CidadeVitrineViewModel
{
    public IReadOnlyList<SlideDto> Slides { get; set; } = Array.Empty<SlideDto>();

    /// <summary>Texto da história (HTML).</summary>
    public string? Texto { get; set; }

    /// <summary>Frase de fecho da seção.</summary>
    public string? Citacao { get; set; }

    /// <summary>Foto principal ("ImagemPrincipal"), já em URL (/arquivo/{id}); vazio usa o primeiro slide.</summary>
    public string? ImagemPrincipal { get; set; }

    /// <summary>Se mostra o título retrô "nossa cidade" dentro do mostruário.</summary>
    public bool ExibirTitulo { get; set; } = true;

    /// <summary>Botão do card (padrão do sistema) — nulo oculta.</summary>
    public string? BotaoHref { get; set; }
    public string? BotaoTexto { get; set; } = "Conheça nossa cidade";

    /// <summary>Evento de analytics do botão (padrão: ver-cidade).</summary>
    public string EventoRastreio { get; set; } = "ver-cidade";
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

