using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;

namespace TurismoEstancia.Services.CulturaGastronomia.Services;

/// <summary>Implementação do serviço de pratos turísticos.</summary>
public class PratoTuristicoService : IPratoTuristicoService
{
    private readonly AppDbContext _db;

    public PratoTuristicoService(AppDbContext db) => _db = db;

    private static readonly Expression<Func<PratoTuristico, PratoTuristicoDto>> ToDto =
        p => new PratoTuristicoDto { Id = p.Id, Nome = p.Nome, Ordem = p.Ordem, Ativo = p.Ativo };

    public async Task<IReadOnlyList<PratoTuristicoDto>> ListarAsync(CancellationToken ct = default) =>
        await _db.PratosTuristicos.AsNoTracking()
            .Where(p => p.Ativo)
            .OrderBy(p => p.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<PratoTuristicoDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.PratosTuristicos.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarAsync(PratoTuristicoDto dto, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            _db.PratosTuristicos.Add(new PratoTuristico { Nome = dto.Nome, Ordem = dto.Ordem, Ativo = true });
        }
        else
        {
            var entidade = await _db.PratosTuristicos.FirstOrDefaultAsync(p => p.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Prato turístico não encontrado.");
            entidade.Nome = dto.Nome;
            entidade.Ordem = dto.Ordem;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.PratosTuristicos.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException("Prato turístico não encontrado.");
        _db.PratosTuristicos.Remove(entidade);
        await _db.SaveChangesAsync(ct);
    }
}
