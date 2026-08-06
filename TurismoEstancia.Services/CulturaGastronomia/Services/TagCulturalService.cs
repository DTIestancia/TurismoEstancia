using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;

namespace TurismoEstancia.Services.CulturaGastronomia.Services;

/// <summary>Implementação do serviço de tags culturais.</summary>
public class TagCulturalService : ITagCulturalService
{
    private readonly AppDbContext _db;

    public TagCulturalService(AppDbContext db) => _db = db;

    private static readonly Expression<Func<TagCultural, TagCulturalDto>> ToDto =
        t => new TagCulturalDto { Id = t.Id, Nome = t.Nome, Ordem = t.Ordem, Ativo = t.Ativo };

    public async Task<IReadOnlyList<TagCulturalDto>> ListarAsync(CancellationToken ct = default) =>
        await _db.TagsCulturais.AsNoTracking()
            .Where(t => t.Ativo)
            .OrderBy(t => t.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<TagCulturalDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.TagsCulturais.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarAsync(TagCulturalDto dto, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            _db.TagsCulturais.Add(new TagCultural { Nome = dto.Nome, Ordem = dto.Ordem, Ativo = true });
        }
        else
        {
            var entidade = await _db.TagsCulturais.FirstOrDefaultAsync(t => t.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Tag cultural não encontrada.");
            entidade.Nome = dto.Nome;
            entidade.Ordem = dto.Ordem;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.TagsCulturais.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new InvalidOperationException("Tag cultural não encontrada.");
        _db.TagsCulturais.Remove(entidade);
        await _db.SaveChangesAsync(ct);
    }
}
