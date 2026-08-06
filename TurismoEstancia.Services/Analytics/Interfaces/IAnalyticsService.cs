using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Analytics.Interfaces;

/// <summary>
/// Serviço de analytics do portal: eventos anônimos (visitas/cliques) são
/// enfileirados e gravados em background; o dashboard consome os agregados.
/// </summary>
public interface IAnalyticsService
{
    /// <summary>Enfileira um evento (visita ou clique) — nunca bloqueia o request.</summary>
    void Registrar(AnalyticsEventoDto dto);

    /// <summary>Resumo agregado do período [de, ate] para o dashboard do Gerenciador.</summary>
    Task<AnalyticsResumoDto> ObterResumoAsync(DateTime de, DateTime ate, CancellationToken ct = default);
}
