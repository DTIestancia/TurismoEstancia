using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.CulturaGastronomia.Interfaces;

/// <summary>Serviço de tags culturais (com emoji, ex.: "🔥 Barco de Fogo").</summary>
public interface ITagCulturalService
{
    Task<IReadOnlyList<TagCulturalDto>> ListarAsync(CancellationToken ct = default);
    Task<TagCulturalDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Salva a tag; <paramref name="imagem"/> opcional substitui a atual.</summary>
    Task SalvarAsync(TagCulturalDto dto, IFormFile? imagem = null, CancellationToken ct = default);

    /// <summary>Oculta a tag do portal (Ativo = false).</summary>
    Task OcultarAsync(int id, CancellationToken ct = default);

    Task ReativarAsync(int id, CancellationToken ct = default);

    /// <summary>Exclusão definitiva da tag (remove também a imagem).</summary>
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
