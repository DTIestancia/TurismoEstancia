using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Conteudo.Interfaces;

namespace TurismoEstancia.Services.Conteudo.Services;

/// <summary>Implementação do serviço de conteúdos do site (blocos de texto).</summary>
public class ConteudoSiteService : IConteudoSiteService
{
    private readonly AppDbContext _db;

    public ConteudoSiteService(AppDbContext db) => _db = db;

    private static readonly Expression<Func<ConteudoSite, ConteudoSiteDto>> ToDto =
        c => new ConteudoSiteDto { Id = c.Id, Chave = c.Chave, Nome = c.Nome, Texto = c.Texto };

    public async Task<IReadOnlyList<ConteudoSiteDto>> ListarAsync(CancellationToken ct = default) =>
        await _db.ConteudosSite.AsNoTracking()
            .OrderBy(c => c.Nome)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<ConteudoSiteDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.ConteudosSite.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    /// <summary>Retorna o texto de uma chave (null se não existir).</summary>
    public async Task<string?> ObterTextoAsync(string chave, CancellationToken ct = default) =>
        await _db.ConteudosSite.AsNoTracking()
            .Where(c => c.Chave == chave)
            .Select(c => c.Texto)
            .FirstOrDefaultAsync(ct);

    /// <summary>Dicionário chave → texto, para compor o HomeViewModel de uma vez.</summary>
    public async Task<Dictionary<string, string?>> ObterDicionarioAsync(CancellationToken ct = default) =>
        await _db.ConteudosSite.AsNoTracking()
            .Select(c => new { c.Chave, c.Texto })
            .ToDictionaryAsync(c => c.Chave, c => c.Texto, ct);

    public async Task SalvarAsync(ConteudoSiteDto dto, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            if (await _db.ConteudosSite.AnyAsync(c => c.Chave == dto.Chave, ct))
                throw new InvalidOperationException("Já existe um conteúdo com esta chave.");

            _db.ConteudosSite.Add(new ConteudoSite { Chave = dto.Chave, Nome = dto.Nome, Texto = dto.Texto });
        }
        else
        {
            var entidade = await _db.ConteudosSite.FirstOrDefaultAsync(c => c.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Conteúdo não encontrado.");
            entidade.Nome = dto.Nome;
            entidade.Texto = dto.Texto;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.ConteudosSite.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new InvalidOperationException("Conteúdo não encontrado.");
        _db.ConteudosSite.Remove(entidade);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SalvarPorChaveAsync(string chave, string nome, string? texto, CancellationToken ct = default)
    {
        var existente = await _db.ConteudosSite.FirstOrDefaultAsync(c => c.Chave == chave, ct);
        if (existente is null)
        {
            _db.ConteudosSite.Add(new ConteudoSite { Chave = chave, Nome = nome, Texto = texto });
        }
        else
        {
            existente.Nome = nome;
            existente.Texto = texto;
        }

        await _db.SaveChangesAsync(ct);
    }
}
