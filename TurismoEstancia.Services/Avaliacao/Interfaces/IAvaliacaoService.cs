using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Avaliacao.Interfaces;

/// <summary>
/// Serviço de avaliações de pontos turísticos.
/// Submissão entra como Aprovada = false (moderação no CMS).
/// </summary>
public interface IAvaliacaoService
{
    /// <summary>Submete uma avaliação do portal (sempre Aprovada = false).</summary>
    Task SubmeterAsync(AvaliacaoDto dto, CancellationToken ct = default);

    /// <summary>Lista avaliações; <paramref name="apenasAprovadas"/> para exibir no portal.</summary>
    Task<IReadOnlyList<AvaliacaoDto>> ListarAsync(bool apenasAprovadas = false, CancellationToken ct = default);

    /// <summary>Lista as avaliações aprovadas de um ponto (para o modal).</summary>
    Task<IReadOnlyList<AvaliacaoDto>> ListarPorPontoAsync(int pontoTuristicoId, bool apenasAprovadas = true, CancellationToken ct = default);

    /// <summary>Aprova uma avaliação pendente (moderação).</summary>
    Task AprovarAsync(int id, CancellationToken ct = default);

    /// <summary>Remove uma avaliação (moderação).</summary>
    Task ExcluirAsync(int id, CancellationToken ct = default);

    /// <summary>Conta as avaliações pendentes (badge do painel).</summary>
    Task<int> ContarPendentesAsync(CancellationToken ct = default);
}
