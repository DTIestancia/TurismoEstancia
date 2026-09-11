using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.MidiaKit.Interfaces;

/// <summary>Serviço dos itens do Mídia kit (/midia-kit).</summary>
public interface IMidiaKitService
{
    /// <summary>Todos os itens (painel), ordenados.</summary>
    Task<IReadOnlyList<MidiaKitItemDto>> ListarAsync(CancellationToken ct = default);

    /// <summary>Somente itens ativos com arquivo (portal), ordenados.</summary>
    Task<IReadOnlyList<MidiaKitItemDto>> ListarAtivosAsync(CancellationToken ct = default);

    Task<MidiaKitItemDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Salva o item; <paramref name="arquivo"/> opcional substitui o atual.</summary>
    Task SalvarAsync(MidiaKitItemDto dto, IFormFile? arquivo = null, CancellationToken ct = default);

    /// <summary>Oculta o item do portal (Ativo = false).</summary>
    Task OcultarAsync(int id, CancellationToken ct = default);

    Task ReativarAsync(int id, CancellationToken ct = default);

    /// <summary>Exclusão definitiva do item (remove também o arquivo).</summary>
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
