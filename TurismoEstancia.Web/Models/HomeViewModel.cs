using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Web.Models;

/// <summary>
/// Agrega todos os dados dinâmicos exibidos na página inicial do portal
/// (a antiga index.html do protótipo, agora com conteúdo do banco).
/// </summary>
public class HomeViewModel
{
    // Conteúdos por chave (textos das seções)
    public Dictionary<string, string?> Conteudos { get; set; } = new();

    // Configurações (guia, vídeo, SEO) — hero com vídeo distinto por breakpoint
    public ConfiguracaoSiteDto? Guia { get; set; }
    public ConfiguracaoSiteDto? VideoInstitucional { get; set; }
    public ConfiguracaoSiteDto? VideoInstitucionalMobile { get; set; }
    public ConfiguracaoSiteDto? TituloSite { get; set; }

    // Hero
    public IReadOnlyList<SlideDto> Slides { get; set; } = Array.Empty<SlideDto>();

    // Estatísticas (história)
    public IReadOnlyList<EstatisticaDto> Estatisticas { get; set; } = Array.Empty<EstatisticaDto>();

    // Cultura & Gastronomia
    public IReadOnlyList<GrupoCulturalDto> GruposCulturais { get; set; } = Array.Empty<GrupoCulturalDto>();
    public IReadOnlyList<PratoTuristicoDto> PratosTuristicos { get; set; } = Array.Empty<PratoTuristicoDto>();
    public IReadOnlyList<TagCulturalDto> TagsCulturais { get; set; } = Array.Empty<TagCulturalDto>();

    // Maravilhas (agrupadas por categoria que apresenta em Maravilhas)
    public IReadOnlyList<CategoriaMaravilhasViewModel> CategoriasMaravilhas { get; set; } = Array.Empty<CategoriaMaravilhasViewModel>();

    // Mapa: POIs + categorias
    public IReadOnlyList<PontoTuristicoDto> PontosParaMapa { get; set; } = Array.Empty<PontoTuristicoDto>();
    public IReadOnlyList<CategoriaPontoTuristicoDto> CategoriasMapa { get; set; } = Array.Empty<CategoriaPontoTuristicoDto>();

    // Agenda (eventos futuros — home mostra só os 3 mais próximos)
    public IReadOnlyList<EventoDto> EventosProximos { get; set; } = Array.Empty<EventoDto>();

    /// <summary>Total de eventos futuros (para decidir se mostra o botão "Ver todas").</summary>
    public int EventosProximosTotal { get; set; }

    // Roteiros
    public IReadOnlyList<RoteiroDto> Roteiros { get; set; } = Array.Empty<RoteiroDto>();

    // Notícias (últimas publicadas, para a seção da home)
    public IReadOnlyList<NoticiaDto> Noticias { get; set; } = Array.Empty<NoticiaDto>();

    // Explorador "Conheça Estância" (História, Cultura, Gastronomia, Experiências)
    public IReadOnlyList<ConhecaEstanciaTab> ConhecaEstancia { get; set; } = Array.Empty<ConhecaEstanciaTab>();

    // Contatos do rodapé
    public IReadOnlyList<ContatoDto> Contatos { get; set; } = Array.Empty<ContatoDto>();

    // Acessórios
    public bool GuiaDisponivel => Guia?.ArquivoId is > 0;
    public long? VideoArquivoId => VideoInstitucional?.ArquivoId;
    public long? VideoArquivoIdMobile => VideoInstitucionalMobile?.ArquivoId;

    /// <summary>Imagem de fundo do mapa (PNG do município) — gerenciada em Secoes/Mapa.</summary>
    public long? MapaImagemArquivoId { get; set; }
    public int MapaImagemZoom { get; set; } = 100;
    public int MapaImagemPosX { get; set; } = 50;
    public int MapaImagemPosY { get; set; } = 50;

    /// <summary>JSON do mapa (categorias + POIs) serializado no controller.</summary>
    public string MapaJson { get; set; } = "{}";

    /// <summary>
    /// As maravilhas com pictograma cadastrado (até 7, na ordem do mapa).
    /// Usada no preloader e na faixa de pictogramas do rodapé — a regra de
    /// coleta fica num só lugar para os dois pontos ficarem sempre em sincronia.
    /// </summary>
    public IReadOnlyList<PontoTuristicoDto> MaravilhasComPictograma
    {
        get
        {
            var comPicto = CategoriasMaravilhas
                .SelectMany(g => g.Pontos)
                .Where(p => p.PictogramaArquivoId is long)
                .OrderBy(p => p.Ordem)
                .Take(7)
                .ToList();
            if (comPicto.Count == 0)
            {
                // Fallback: pontos do mapa que possuem pictograma.
                comPicto = PontosParaMapa
                    .Where(p => p.PictogramaArquivoId is long)
                    .OrderBy(p => p.Ordem)
                    .Take(7)
                    .ToList();
            }
            return comPicto;
        }
    }
}

/// <summary>Uma categoria + seus pontos (seção 7 Maravilhas).</summary>
public class CategoriaMaravilhasViewModel
{
    public CategoriaPontoTuristicoDto Categoria { get; set; } = null!;
    public IReadOnlyList<PontoTuristicoDto> Pontos { get; set; } = Array.Empty<PontoTuristicoDto>();
}

/// <summary>
/// Uma aba da seção "Conheça Estância" (História, Cultura, Gastronomia,
/// Experiências) com seus itens exibidos no carrossel de tela cheia.
/// </summary>
public class ConhecaEstanciaTab
{
    public string Chave { get; set; } = "";
    public string Rotulo { get; set; } = "";
    public string? Icone { get; set; }
    public IReadOnlyList<ConhecaEstanciaItem> Itens { get; set; } = Array.Empty<ConhecaEstanciaItem>();
}

/// <summary>Um item da seção (foto + título + descrição + página de detalhe).</summary>
public class ConhecaEstanciaItem
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string? Descricao { get; set; }
    public long? ImagemArquivoId { get; set; }
    public int ImagemZoom { get; set; } = 100;
    public int ImagemPosicaoX { get; set; } = 50;
    public int ImagemPosicaoY { get; set; } = 50;
    public string? Url { get; set; }
}
