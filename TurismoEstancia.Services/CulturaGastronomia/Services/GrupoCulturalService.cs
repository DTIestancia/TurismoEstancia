using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;

namespace TurismoEstancia.Services.CulturaGastronomia.Services;

/// <summary>Implementação do serviço de grupos culturais.</summary>
public class GrupoCulturalService : IGrupoCulturalService
{
    private readonly AppDbContext _db;

    public GrupoCulturalService(AppDbContext db) => _db = db;

    private static readonly Expression<Func<GrupoCultural, GrupoCulturalDto>> ToDto =
        g => new GrupoCulturalDto { Id = g.Id, Nome = g.Nome, Ordem = g.Ordem, Ativo = g.Ativo };

    public async Task<IReadOnlyList<GrupoCulturalDto>> ListarAsync(CancellationToken ct = default) =>
        await _db.GruposCulturais.AsNoTracking()
            .Where(g => g.Ativo)
            .OrderBy(g => g.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<GrupoCulturalDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.GruposCulturais.AsNoTracking()
            .Where(g => g.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarAsync(GrupoCulturalDto dto, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            _db.GruposCulturais.Add(new GrupoCultural { Nome = dto.Nome, Ordem = dto.Ordem, Ativo = true });
        }
        else
        {
            var entidade = await _db.GruposCulturais.FirstOrDefaultAsync(g => g.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Grupo cultural não encontrado.");
            entidade.Nome = dto.Nome;
            entidade.Ordem = dto.Ordem;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.GruposCulturais.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new InvalidOperationException("Grupo cultural não encontrado.");
        _db.GruposCulturais.Remove(entidade);
        await _db.SaveChangesAsync(ct);
    }
}
