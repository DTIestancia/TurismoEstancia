using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Conteudo.Interfaces;

namespace TurismoEstancia.Web.Infrastructure;

/// <summary>
/// Decorator de <see cref="IConfiguracaoSiteService"/> com cache **por request**:
/// a primeira leitura (por chave ou listagem) carrega todas as configurações de
/// uma única vez (são poucas linhas) e atende as demais leituras pelo dicionário.
/// Assim o SEO, o header, o rodapé, o favicon e os controllers leem a configuração
/// com **1 consulta por request** — sem nunca servir dado velho, pois o cache
/// morre junto com o request. Salvar/excluir invalidam o cache na hora.
/// </summary>
public sealed class ConfiguracaoSiteCache : IConfiguracaoSiteService
{
    private readonly IConfiguracaoSiteService _inner;
    private readonly Dictionary<string, ConfiguracaoSiteDto?> _porChave = new();
    private bool _carregado;

    public ConfiguracaoSiteCache(IConfiguracaoSiteService inner) => _inner = inner;

    public async Task<IReadOnlyList<ConfiguracaoSiteDto>> ListarAsync(CancellationToken ct = default)
    {
        await CarregarAsync(ct);
        // Preserva a ordenação por Nome do serviço interno (a tabela do Gerenciador depende dela).
        return _porChave.Values
            .Where(v => v is not null)
            .Cast<ConfiguracaoSiteDto>()
            .OrderBy(c => c.Nome)
            .ToList();
    }

    public Task<ConfiguracaoSiteDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        _inner.ObterPorIdAsync(id, ct);

    public async Task<ConfiguracaoSiteDto?> ObterPorChaveAsync(string chave, CancellationToken ct = default)
    {
        if (_porChave.TryGetValue(chave, out var dto)) return dto;

        await CarregarAsync(ct);

        if (_porChave.TryGetValue(chave, out dto)) return dto;

        // Chave inexistente no banco: guarda null para não repetir a busca.
        _porChave[chave] = null;
        return null;
    }

    public async Task SalvarAsync(ConfiguracaoSiteDto dto, IFormFile? arquivo, CancellationToken ct = default)
    {
        await _inner.SalvarAsync(dto, arquivo, ct);
        Invalidar();
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        await _inner.ExcluirAsync(id, ct);
        Invalidar();
    }

    /// <summary>Carrega todas as configurações uma única vez por request.</summary>
    private async Task CarregarAsync(CancellationToken ct)
    {
        if (_carregado) return;

        foreach (var configuracao in await _inner.ListarAsync(ct))
            _porChave[configuracao.Chave] = configuracao;
        _carregado = true;
    }

    /// <summary>Limpa o cache — a próxima leitura recarrega as configurações.</summary>
    private void Invalidar()
    {
        _porChave.Clear();
        _carregado = false;
    }
}
