using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Conteudo.Interfaces;

/// <summary>Serviço de configurações do site (slots únicos: guia PDF, vídeo, SEO).</summary>
public interface IConfiguracaoSiteService
{
    Task<IReadOnlyList<ConfiguracaoSiteDto>> ListarAsync(CancellationToken ct = default);
    Task<ConfiguracaoSiteDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<ConfiguracaoSiteDto?> ObterPorChaveAsync(string chave, CancellationToken ct = default);
    Task SalvarAsync(ConfiguracaoSiteDto dto, IFormFile? arquivo, CancellationToken ct = default);
    Task ExcluirAsync(int id, CancellationToken ct = default);
}
