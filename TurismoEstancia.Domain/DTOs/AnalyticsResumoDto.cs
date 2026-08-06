namespace TurismoEstancia.Domain.DTOs;

/// <summary>Resumo agregado de analytics para o dashboard do Gerenciador.</summary>
public class AnalyticsResumoDto
{
    public long Visitas { get; set; }
    public long VisitantesUnicos { get; set; }
    public long Cliques { get; set; }
    public long VisitasHoje { get; set; }

    /// <summary>Série visitas por dia (para o gráfico de linha).</summary>
    public List<AnalyticsSerieDiaDto> VisitasPorDia { get; set; } = new();

    /// <summary>Top 10 páginas mais visitadas (rota).</summary>
    public List<AnalyticsContagemDto> TopPaginas { get; set; } = new();

    /// <summary>Top referrers (domínios de origem).</summary>
    public List<AnalyticsContagemDto> TopReferrers { get; set; } = new();

    /// <summary>Visitas por dispositivo (Desktop/Mobile/Tablet).</summary>
    public List<AnalyticsContagemDto> Dispositivos { get; set; } = new();

    /// <summary>Visitas por fonte (Buscas/Redes sociais/Direto/Outros).</summary>
    public List<AnalyticsContagemDto> Fontes { get; set; } = new();

    /// <summary>Top maravilhas mais clicadas (evento ver-maravilha).</summary>
    public List<AnalyticsContagemDto> TopMaravilhas { get; set; } = new();

    /// <summary>Cliques por tipo de evento.</summary>
    public List<AnalyticsContagemDto> TopEventos { get; set; } = new();
}

/// <summary>Ponto de uma série temporal.</summary>
public class AnalyticsSerieDiaDto
{
    public DateTime Data { get; set; }
    public int Quantidade { get; set; }
}

/// <summary>Rótulo + contagem (páginas, referrers, dispositivos...).</summary>
public class AnalyticsContagemDto
{
    public string Rotulo { get; set; } = "";
    public int Quantidade { get; set; }
}
