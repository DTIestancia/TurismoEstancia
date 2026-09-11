using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Turismo.Interfaces;

/// <summary>Serviço de estatísticas da seção história.</summary>
public interface IEstatisticaService
{
    Task<IReadOnlyList<EstatisticaDto>> ListarAsync(CancellationToken ct = default);
    Task<EstatisticaDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task SalvarAsync(EstatisticaDto dto, CancellationToken ct = default);

    /// <summary>Oculta a estatística do portal (Ativo = false).</summary>
    Task OcultarAsync(int id, CancellationToken ct = default);

    Task ReativarAsync(int id, CancellationToken ct = default);

    /// <summary>Exclusão definitiva da estatística.</summary>
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
