using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.CulturaGastronomia.Interfaces;

/// <summary>Serviço de grupos culturais (Reisado, Cacumbi, Batucada...).</summary>
public interface IGrupoCulturalService
{
    Task<IReadOnlyList<GrupoCulturalDto>> ListarAsync(CancellationToken ct = default);
    Task<GrupoCulturalDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Salva o grupo; <paramref name="imagem"/> opcional substitui a atual.</summary>
    Task SalvarAsync(GrupoCulturalDto dto, IFormFile? imagem = null, CancellationToken ct = default);

    /// <summary>Oculta o grupo do portal (Ativo = false).</summary>
    Task OcultarAsync(int id, CancellationToken ct = default);

    Task ReativarAsync(int id, CancellationToken ct = default);

    /// <summary>Exclusão definitiva do grupo (remove também a imagem).</summary>
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
