using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Conteudo.Interfaces;

/// <summary>Serviço de blocos de texto do portal (por chave única).</summary>
public interface IConteudoSiteService
{
    Task<IReadOnlyList<ConteudoSiteDto>> ListarAsync(CancellationToken ct = default);
    Task<ConteudoSiteDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<string?> ObterTextoAsync(string chave, CancellationToken ct = default);
    Task<Dictionary<string, string?>> ObterDicionarioAsync(CancellationToken ct = default);
    Task SalvarAsync(ConteudoSiteDto dto, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
