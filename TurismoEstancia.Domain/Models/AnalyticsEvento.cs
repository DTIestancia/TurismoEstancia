namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Evento de analytics do portal (visita de página ou clique).
/// Anônimo por design (LGPD): nenhum IP ou dado pessoal é armazenado — a
/// identificação de visitante único usa um cookie de sessão UUID (primeira
/// parte, sem consentimento exigido). Gravado em fila em background.
/// </summary>
public class AnalyticsEvento
{
    public long Id { get; set; }

    /// <summary>Data/hora do evento (GETDATE()).</summary>
    public DateTime Data { get; set; }

    /// <summary>Tipo: "Visita" (página aberta) ou "Clique" (interação rastreada).</summary>
    public string Tipo { get; set; } = "Visita";

    /// <summary>Caminho da página (ex.: "/lugares/18/praia-do-saco").</summary>
    public string Rota { get; set; } = null!;

    /// <summary>Título amigável da página (preenchido por convenção de rota).</summary>
    public string? Titulo { get; set; }

    /// <summary>Domínio de origem (referer) — ex.: www.google.com.br. Vazio = acesso direto.</summary>
    public string? RefererHost { get; set; }

    /// <summary>Identificador de sessão anônima (cookie UUID) para contagem de visitantes únicos.</summary>
    public string SessaoId { get; set; } = null!;

    /// <summary>Dispositivo: Desktop | Mobile | Tablet (inferido do User-Agent).</summary>
    public string Dispositivo { get; set; } = "Desktop";

    /// <summary>Chave do clique rastreado (ex.: "ver-maravilha", "mapa-poi", "noticia").</summary>
    public string? Evento { get; set; }

    /// <summary>Id da entidade clicada (ex.: ponto turístico).</summary>
    public int? EntidadeId { get; set; }

    /// <summary>Nome da entidade clicada (ex.: "Praia do Saco") — para rankings.</summary>
    public string? EntidadeNome { get; set; }
}
