using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Services.Conteudo.Services;

/// <summary>Implementação do serviço de configurações do site (guia, vídeo, SEO).</summary>
public class ConfiguracaoSiteService : IConfiguracaoSiteService
{
    private readonly AppDbContext _db;
    private readonly IArquivoService _arquivos;

    public ConfiguracaoSiteService(AppDbContext db, IArquivoService arquivos)
    {
        _db = db;
        _arquivos = arquivos;
    }

    private static readonly Expression<Func<ConfiguracaoSite, ConfiguracaoSiteDto>> ToDto =
        c => new ConfiguracaoSiteDto
        {
            Id = c.Id,
            Chave = c.Chave,
            Nome = c.Nome,
            Tipo = c.Tipo,
            ValorTexto = c.ValorTexto,
            ArquivoId = c.ArquivoId,
            ArquivoNome = c.Arquivo != null ? c.Arquivo.Nome : null
        };

    public async Task<IReadOnlyList<ConfiguracaoSiteDto>> ListarAsync(CancellationToken ct = default) =>
        await _db.ConfiguracoesSite.AsNoTracking()
            .OrderBy(c => c.Nome)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<ConfiguracaoSiteDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.ConfiguracoesSite.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task<ConfiguracaoSiteDto?> ObterPorChaveAsync(string chave, CancellationToken ct = default) =>
        await _db.ConfiguracoesSite.AsNoTracking()
            .Where(c => c.Chave == chave)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarAsync(ConfiguracaoSiteDto dto, IFormFile? arquivo, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            if (await _db.ConfiguracoesSite.AnyAsync(c => c.Chave == dto.Chave, ct))
                throw new InvalidOperationException("Já existe uma configuração com esta chave.");

            _db.ConfiguracoesSite.Add(new ConfiguracaoSite
            {
                Chave = dto.Chave,
                Nome = dto.Nome,
                Tipo = dto.Tipo,
                ValorTexto = dto.ValorTexto,
                // O upload escolhido no formulário também vale na criação —
                // antes o arquivo era descartado e a config nascia sem ArquivoId.
                ArquivoId = arquivo is { Length: > 0 }
                    ? await _arquivos.SalvarAsync(arquivo, ct)
                    : null
            });
        }
        else
        {
            var entidade = await _db.ConfiguracoesSite.FirstOrDefaultAsync(c => c.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Configuração não encontrada.");

            entidade.Nome = dto.Nome;
            entidade.Tipo = dto.Tipo;
            entidade.ValorTexto = dto.ValorTexto;

            long? antigoId = null;
            if (arquivo is { Length: > 0 })
            {
                antigoId = entidade.ArquivoId;
                entidade.ArquivoId = await _arquivos.SalvarAsync(arquivo, ct);
            }

            await _db.SaveChangesAsync(ct);

            // Remove o arquivo antigo só após o commit.
            if (antigoId.HasValue)
                await _arquivos.ExcluirAsync(antigoId.Value, ct);
            return;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.ConfiguracoesSite.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new InvalidOperationException("Configuração não encontrada.");
        var arquivoId = entidade.ArquivoId;
        _db.ConfiguracoesSite.Remove(entidade);
        await _db.SaveChangesAsync(ct);
        if (arquivoId.HasValue)
            await _arquivos.ExcluirAsync(arquivoId.Value, ct);
    }
}
