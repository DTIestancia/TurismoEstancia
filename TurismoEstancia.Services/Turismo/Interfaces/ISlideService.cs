using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Turismo.Interfaces;

/// <summary>Serviço de slides do hero.</summary>
public interface ISlideService
{
    Task<IReadOnlyList<SlideDto>> ListarAsync(CancellationToken ct = default);
    Task<SlideDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task SalvarAsync(SlideDto dto, IFormFile? imagem, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
