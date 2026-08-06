using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Avaliacao.Interfaces;
using AvaliacaoEntity = TurismoEstancia.Domain.Models.Avaliacao;

namespace TurismoEstancia.Services.Avaliacao.Services;

/// <summary>Implementação do serviço de avaliações.</summary>
public class AvaliacaoService : IAvaliacaoService
{
    private readonly AppDbContext _db;

    public AvaliacaoService(AppDbContext db) => _db = db;

    private static readonly Expression<Func<AvaliacaoEntity, AvaliacaoDto>> ToDto =
        a => new AvaliacaoDto
        {
            Id = a.Id,
            PontoTuristicoId = a.PontoTuristicoId,
            PontoTuristicoNome = a.PontoTuristico != null ? a.PontoTuristico.Nome : null,
            Nome = a.Nome,
            Nota = a.Nota,
            Comentario = a.Comentario,
            Data = a.Data,
            Aprovada = a.Aprovada
        };

    public async Task SubmeterAsync(AvaliacaoDto dto, CancellationToken ct = default)
    {
        if (dto.Nota < 1 || dto.Nota > 5)
            throw new InvalidOperationException("A nota deve estar entre 1 e 5.");

        var pontoExiste = await _db.PontosTuristicos.AnyAsync(p => p.Id == dto.PontoTuristicoId, ct);
        if (!pontoExiste)
            throw new InvalidOperationException("Ponto turístico não encontrado.");

        _db.Avaliacoes.Add(new AvaliacaoEntity
        {
            PontoTuristicoId = dto.PontoTuristicoId,
            Nome = dto.Nome,
            Nota = dto.Nota,
            Comentario = dto.Comentario,
            Aprovada = false // sempre entra para moderação
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AvaliacaoDto>> ListarAsync(bool apenasAprovadas = false, CancellationToken ct = default)
    {
        IQueryable<AvaliacaoEntity> query = _db.Avaliacoes.AsNoTracking().Include(a => a.PontoTuristico);
        if (apenasAprovadas)
            query = query.Where(a => a.Aprovada);

        return await query
            .OrderByDescending(a => a.Data)
            .Select(ToDto)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AvaliacaoDto>> ListarPorPontoAsync(int pontoTuristicoId, bool apenasAprovadas = true, CancellationToken ct = default)
    {
        var query = _db.Avaliacoes.AsNoTracking().Where(a => a.PontoTuristicoId == pontoTuristicoId);
        if (apenasAprovadas)
            query = query.Where(a => a.Aprovada);

        return await query
            .OrderByDescending(a => a.Data)
            .Select(ToDto)
            .ToListAsync(ct);
    }

    public async Task AprovarAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.Avaliacoes.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new InvalidOperationException("Avaliação não encontrada.");
        entidade.Aprovada = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.Avaliacoes.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new InvalidOperationException("Avaliação não encontrada.");
        _db.Avaliacoes.Remove(entidade);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> ContarPendentesAsync(CancellationToken ct = default) =>
        await _db.Avaliacoes.CountAsync(a => !a.Aprovada, ct);
}
