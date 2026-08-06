using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.CulturaGastronomia.Interfaces;

/// <summary>Serviço de grupos culturais (Reisado, Cacumbi, Batucada...).</summary>
public interface IGrupoCulturalService
{
    Task<IReadOnlyList<GrupoCulturalDto>> ListarAsync(CancellationToken ct = default);
    Task<GrupoCulturalDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task SalvarAsync(GrupoCulturalDto dto, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
