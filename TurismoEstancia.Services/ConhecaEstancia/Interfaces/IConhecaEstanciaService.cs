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

    /// <summary>Oculta o item do portal (Ativo = false).</summary>
    Task OcultarAsync(int id, CancellationToken ct = default);

    Task ReativarAsync(int id, CancellationToken ct = default);

    /// <summary>Exclusão definitiva do item (remove também a imagem).</summary>
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
