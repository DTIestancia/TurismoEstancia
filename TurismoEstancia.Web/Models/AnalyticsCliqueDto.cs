using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Web.Models;

/// <summary>
/// Payload do beacon de cliques (POST /api/analytics/event).
/// Sessão anônima vem do cookie — nunca é enviada pelo cliente.
/// Limites de tamanho evitam lixo no banco (endpoint público).
/// </summary>
public class AnalyticsCliqueDto
{
    /// <summary>Chave do clique (ex.: "ver-maravilha", "mapa-poi", "noticia").</summary>
    [MaxLength(60)]
    public string? Evento { get; set; }

    /// <summary>Id da entidade clicada (ponto, notícia, roteiro...).</summary>
    public int? EntidadeId { get; set; }

    /// <summary>Nome da entidade clicada (para rankings).</summary>
    [MaxLength(150)]
    public string? EntidadeNome { get; set; }

    /// <summary>Caminho da página onde o clique ocorreu.</summary>
    [MaxLength(255)]
    public string? Rota { get; set; }
}
