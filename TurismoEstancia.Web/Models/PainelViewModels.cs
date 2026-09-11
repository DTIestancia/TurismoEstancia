namespace TurismoEstancia.Web.Models;

using TurismoEstancia.Domain.DTOs;

/// <summary>Card de contagem exibido nos dashboards do painel.</summary>
public class PainelStatViewModel
{
    public string Rotulo { get; set; } = string.Empty;
    public string Icone { get; set; } = "info";
    public int Valor { get; set; }

    /// <summary>Link do card (ex.: "/Gerenciador/Eventos"). Null = sem link.</summary>
    public string? Url { get; set; }
}

/// <summary>
/// Linha da moderação unificada de avaliações (Gerenciador › Avaliações):
/// junta as avaliações dos pontos turísticos (mapa) e as do "Planeje sua
/// viagem", indicando a origem de cada uma.
/// </summary>
public class AvaliacaoGerenciadorViewModel
{
    /// <summary>Origem: "ponto" (mapa/maravilhas) ou "planeje".</summary>
    public string Origem { get; set; } = "ponto";

    public int Id { get; set; }

    /// <summary>Nome do ponto turístico ou do local do Planeje.</summary>
    public string Local { get; set; } = string.Empty;

    public string Visitante { get; set; } = "Anônimo";
    public int Nota { get; set; }
    public string? Comentario { get; set; }
    public DateTime Data { get; set; }
    public bool Aprovada { get; set; }
}

/// <summary>
/// Dashboard de análises do Gerenciador: KPIs de audiência (anônimos),
/// gráficos, rankings, crescimento da newsletter e SEO.
/// </summary>
public class DashboardAnalyticsViewModel
{
    /// <summary>Período selecionado em dias (7, 30 ou 90).</summary>
    public int PeriodoDias { get; set; } = 30;

    public DateTime De { get; set; }
    public DateTime Ate { get; set; }

    public AnalyticsResumoDto Resumo { get; set; } = new();

    /// <summary>Novas inscrições na newsletter no período.</summary>
    public int NewsletterNoPeriodo { get; set; }

    /// <summary>Total de inscrições ativas hoje.</summary>
    public int NewsletterAtivas { get; set; }

    /// <summary>Contadores de conteúdo publicados (KPIs secundários).</summary>
    public IReadOnlyList<PainelStatViewModel> Conteudos { get; set; } = Array.Empty<PainelStatViewModel>();

    /// <summary>Número real de rotas públicas no sitemap.</summary>
    public int RotasIndexaveis { get; set; }

    /// <summary>Configurações de SEO atuais (título + meta descrição).</summary>
    public string? SeoTitulo { get; set; }
    public string? SeoDescricao { get; set; }

    /// <summary>Categorias da galeria para o filtro do ranking de fotos (inclui inativas).</summary>
    public IReadOnlyList<GaleriaCategoriaDto> GaleriaCategorias { get; set; } = Array.Empty<GaleriaCategoriaDto>();

    /// <summary>Categoria selecionada no filtro do ranking (null = todas as categorias).</summary>
    public int? GaleriaCategoriaId { get; set; }

    /// <summary>Visitas no período anterior (mesmo tamanho) — comparativo dos KPIs.</summary>
    public long VisitasAnteriores { get; set; }

    /// <summary>Cliques no período anterior (mesmo tamanho) — comparativo dos KPIs.</summary>
    public long CliquesAnteriores { get; set; }

    /// <summary>Avaliações aguardando moderação (pontos + planeje).</summary>
    public int AvaliacoesPendentes { get; set; }

    /// <summary>Itens ocultos (Ativo = false) somando os módulos de conteúdo.</summary>
    public int ItensInativos { get; set; }
}
