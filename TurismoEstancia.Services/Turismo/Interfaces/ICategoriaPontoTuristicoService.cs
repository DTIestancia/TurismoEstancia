using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Turismo.Interfaces;

/// <summary>Serviço de categoria de ponto turístico.</summary>
public interface ICategoriaPontoTuristicoService
{
    Task<IReadOnlyList<CategoriaPontoTuristicoDto>> ListarAsync(bool incluirInativos = false, CancellationToken ct = default);
    Task<CategoriaPontoTuristicoDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task SalvarAsync(CategoriaPontoTuristicoDto dto, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
