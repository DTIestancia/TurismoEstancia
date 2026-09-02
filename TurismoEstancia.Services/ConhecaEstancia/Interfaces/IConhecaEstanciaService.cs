using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.ConhecaEstancia.Interfaces;

/// <summary>
/// Serviço dos itens da seção "Conheça Estância" (conteúdo exclusivo da
/// seção, sem vínculo com pontos turísticos, grupos ou pratos).
/// </summary>
public interface IConhecaEstanciaService
{
    /// <summary>Todos os itens (painel), ordenados por categoria e ordem.</summary>
    Task<IReadOnlyList<ConhecaEstanciaItemDto>> ListarAsync(CancellationToken ct = default);

    /// <summary>Somente itens ativos (portal), ordenados por categoria e ordem.</summary>
    Task<IReadOnlyList<ConhecaEstanciaItemDto>> ListarAtivosAsync(CancellationToken ct = default);

    Task<ConhecaEstanciaItemDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Salva o item; <paramref name="imagem"/> opcional substitui a atual.</summary>
    Task SalvarAsync(ConhecaEstanciaItemDto dto, IFormFile? imagem = null, CancellationToken ct = default);

    Task ExcluirAsync(int id, CancellationToken ct = default);
}
