using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.CulturaGastronomia.Interfaces;

/// <summary>Serviço de pratos turísticos.</summary>
public interface IPratoTuristicoService
{
    Task<IReadOnlyList<PratoTuristicoDto>> ListarAsync(CancellationToken ct = default);
    Task<PratoTuristicoDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task SalvarAsync(PratoTuristicoDto dto, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
