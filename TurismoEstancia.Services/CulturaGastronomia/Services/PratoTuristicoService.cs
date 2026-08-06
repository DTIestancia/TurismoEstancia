using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Services.CulturaGastronomia.Services;

/// <summary>Implementação do serviço de pratos turísticos.</summary>
public class PratoTuristicoService : IPratoTuristicoService
{
    private readonly AppDbContext _db;
    private readonly IArquivoService _arquivos;

    public PratoTuristicoService(AppDbContext db, IArquivoService arquivos)
    {
        _db = db;
        _arquivos = arquivos;
    }

    private static readonly Expression<Func<PratoTuristico, PratoTuristicoDto>> ToDto =
        p => new PratoTuristicoDto
        {
            Id = p.Id,
            Nome = p.Nome,
            Descricao = p.Descricao,
            ImagemArquivoId = p.ImagemArquivoId,
            Ordem = p.Ordem,
            Ativo = p.Ativo
        };

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

    public async Task SalvarAsync(PratoTuristicoDto dto, IFormFile? imagem = null, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            var novo = new PratoTuristico
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Ordem = dto.Ordem,
                Ativo = true
            };

            if (imagem is { Length: > 0 })
                novo.ImagemArquivoId = await _arquivos.SalvarAsync(imagem, ct);

            _db.PratosTuristicos.Add(novo);
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            var entidade = await _db.PratosTuristicos.FirstOrDefaultAsync(p => p.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Prato turístico não encontrado.");

            entidade.Nome = dto.Nome;
            entidade.Descricao = dto.Descricao;
            entidade.Ordem = dto.Ordem;

            long? antigoId = null;
            if (imagem is { Length: > 0 })
            {
                antigoId = entidade.ImagemArquivoId;
                entidade.ImagemArquivoId = await _arquivos.SalvarAsync(imagem, ct);
            }

            await _db.SaveChangesAsync(ct);

            if (antigoId.HasValue)
                await _arquivos.ExcluirAsync(antigoId.Value, ct);
        }
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.PratosTuristicos.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException("Prato turístico não encontrado.");
        var imagemId = entidade.ImagemArquivoId;
        _db.PratosTuristicos.Remove(entidade);
        await _db.SaveChangesAsync(ct);
        if (imagemId.HasValue)
            await _arquivos.ExcluirAsync(imagemId.Value, ct);
    }
}
