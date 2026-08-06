namespace TurismoEstancia.Domain.DTOs;

/// <summary>Evento de analytics anônimo (LGPD-safe: sem IP/dados pessoais).</summary>
public class AnalyticsEventoDto
{
    public DateTime Data { get; set; } = DateTime.Now;

    /// <summary>Tipo: "Visita" (página aberta) ou "Clique" (interação).</summary>
    public string Tipo { get; set; } = "Visita";

    /// <summary>Caminho da página (ex.: "/lugares/18/praia-do-saco").</summary>
    public string Rota { get; set; } = null!;

    /// <summary>Título amigável da página.</summary>
    public string? Titulo { get; set; }

    /// <summary>Domínio de origem (referer); vazio = acesso direto.</summary>
    public string? RefererHost { get; set; }

    /// <summary>Identificador de sessão anônima (cookie UUID).</summary>
    public string SessaoId { get; set; } = null!;

    /// <summary>Dispositivo: Desktop | Mobile | Tablet.</summary>
    public string Dispositivo { get; set; } = "Desktop";

    /// <summary>Chave do clique rastreado (ex.: "ver-maravilha", "mapa-poi").</summary>
    public string? Evento { get; set; }

    /// <summary>Id da entidade clicada.</summary>
    public int? EntidadeId { get; set; }

    /// <summary>Nome da entidade clicada (para rankings).</summary>
    public string? EntidadeNome { get; set; }
}
