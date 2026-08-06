using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Services.Turismo.Services;

/// <summary>Implementação do serviço de estatísticas.</summary>
public class EstatisticaService : IEstatisticaService
{
    private readonly AppDbContext _db;

    public EstatisticaService(AppDbContext db) => _db = db;

    private static readonly Expression<Func<Estatistica, EstatisticaDto>> ToDto =
        e => new EstatisticaDto
        {
            Id = e.Id,
            Valor = e.Valor,
            Legenda = e.Legenda,
            Ordem = e.Ordem,
            Ativo = e.Ativo
        };

    public async Task<IReadOnlyList<EstatisticaDto>> ListarAsync(CancellationToken ct = default) =>
        await _db.Estatisticas.AsNoTracking()
            .Where(e => e.Ativo)
            .OrderBy(e => e.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<EstatisticaDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.Estatisticas.AsNoTracking()
            .Where(e => e.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarAsync(EstatisticaDto dto, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            _db.Estatisticas.Add(new Estatistica
            {
                Valor = dto.Valor,
                Legenda = dto.Legenda,
                Ordem = dto.Ordem,
                Ativo = true
            });
        }
        else
        {
            var entidade = await _db.Estatisticas.FirstOrDefaultAsync(e => e.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Estatística não encontrada.");
            entidade.Valor = dto.Valor;
            entidade.Legenda = dto.Legenda;
            entidade.Ordem = dto.Ordem;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.Estatisticas.FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new InvalidOperationException("Estatística não encontrada.");
        _db.Estatisticas.Remove(entidade);
        await _db.SaveChangesAsync(ct);
    }
}
