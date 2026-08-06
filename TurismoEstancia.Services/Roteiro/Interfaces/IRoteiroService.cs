using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Roteiro.Interfaces;

/// <summary>Serviço de roteiros turísticos (com itens Dia/Ordem/Observacao).</summary>
public interface IRoteiroService
{
    Task<IReadOnlyList<RoteiroDto>> ListarAsync(CancellationToken ct = default);
    Task<RoteiroDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task SalvarAsync(RoteiroDto dto, IFormFile? imagem, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
