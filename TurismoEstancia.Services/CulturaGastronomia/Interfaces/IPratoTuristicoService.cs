using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.CulturaGastronomia.Interfaces;

/// <summary>Serviço de pratos turísticos.</summary>
public interface IPratoTuristicoService
{
    Task<IReadOnlyList<PratoTuristicoDto>> ListarAsync(CancellationToken ct = default);
    Task<PratoTuristicoDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Salva o prato; <paramref name="imagem"/> opcional substitui a atual.</summary>
    Task SalvarAsync(PratoTuristicoDto dto, IFormFile? imagem = null, CancellationToken ct = default);

    Task ExcluirAsync(int id, CancellationToken ct = default);
}
