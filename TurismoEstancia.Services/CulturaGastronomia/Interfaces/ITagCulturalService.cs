using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.CulturaGastronomia.Interfaces;

/// <summary>Serviço de tags culturais (com emoji, ex.: "🔥 Barco de Fogo").</summary>
public interface ITagCulturalService
{
    Task<IReadOnlyList<TagCulturalDto>> ListarAsync(CancellationToken ct = default);
    Task<TagCulturalDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task SalvarAsync(TagCulturalDto dto, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
