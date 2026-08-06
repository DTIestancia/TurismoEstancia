using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Comunicacao.Interfaces;

/// <summary>Serviço de notícias (slug único, Publicada controla exibição).</summary>
public interface INoticiaService
{
    /// <summary>Lista notícias; <paramref name="apenasPublicadas"/> para o portal.</summary>
    Task<IReadOnlyList<NoticiaDto>> ListarAsync(bool apenasPublicadas = false, CancellationToken ct = default);

    Task<NoticiaDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Obtém por slug (apenas publicada).</summary>
    Task<NoticiaDto?> ObterPorSlugAsync(string slug, CancellationToken ct = default);

    Task SalvarAsync(NoticiaDto dto, IFormFile? imagem, CancellationToken ct = default);

    /// <summary>Exclusão lógica (Ativo = false).</summary>
    Task ExcluirAsync(int id, CancellationToken ct = default);

    /// <summary>Gera um slug único a partir do título.</summary>
    Task<string> GerarSlugAsync(string titulo, int? ignorarId = null, CancellationToken ct = default);
}
