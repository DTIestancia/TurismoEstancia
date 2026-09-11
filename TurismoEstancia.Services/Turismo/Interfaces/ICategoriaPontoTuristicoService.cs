using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Turismo.Interfaces;

/// <summary>Serviço de categoria de ponto turístico.</summary>
public interface ICategoriaPontoTuristicoService
{
    Task<IReadOnlyList<CategoriaPontoTuristicoDto>> ListarAsync(bool incluirInativos = false, CancellationToken ct = default);
    Task<CategoriaPontoTuristicoDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task SalvarAsync(CategoriaPontoTuristicoDto dto, IFormFile? icone = null, CancellationToken ct = default);

    /// <summary>Oculta a categoria do portal (Ativo = false).</summary>
    Task OcultarAsync(int id, CancellationToken ct = default);

    Task ReativarAsync(int id, CancellationToken ct = default);

    /// <summary>Exclusão definitiva (bloqueada se houver pontos vinculados).</summary>
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
