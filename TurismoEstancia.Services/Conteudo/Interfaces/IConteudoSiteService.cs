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

    /// <summary>
    /// Cria ou atualiza o texto de uma chave (upsert por chave) — usado pelas
    /// telas de área do portal (Hero, Nossa Cidade, Cultura...) que editam os
    /// textos da seção num formulário só, sem precisar saber o Id do registro.
    /// </summary>
    Task SalvarPorChaveAsync(string chave, string nome, string? texto, CancellationToken ct = default);
}
