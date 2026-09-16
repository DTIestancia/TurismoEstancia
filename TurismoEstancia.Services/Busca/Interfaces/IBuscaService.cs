using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Busca.Interfaces;

/// <summary>Busca pública do portal (maravilhas, planeje, notícias, eventos, conheça, gastronomia, grupos).</summary>
public interface IBuscaService
{
    /// <summary>Busca o termo em todas as seções (AND entre palavras, sem acento). Lista vazia quando vazio.</summary>
    Task<BuscaResultadoDto> BuscarAsync(string? termo, CancellationToken ct = default);
}
