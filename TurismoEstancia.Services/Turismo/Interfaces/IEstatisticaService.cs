using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Turismo.Interfaces;

/// <summary>Serviço de estatísticas da seção história.</summary>
public interface IEstatisticaService
{
    Task<IReadOnlyList<EstatisticaDto>> ListarAsync(CancellationToken ct = default);
    Task<EstatisticaDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task SalvarAsync(EstatisticaDto dto, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
