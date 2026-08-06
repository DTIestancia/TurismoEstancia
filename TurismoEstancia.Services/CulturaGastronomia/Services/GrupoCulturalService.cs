using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Services.CulturaGastronomia.Services;

/// <summary>Implementação do serviço de grupos culturais.</summary>
public class GrupoCulturalService : IGrupoCulturalService
{
    private readonly AppDbContext _db;
    private readonly IArquivoService _arquivos;

    public GrupoCulturalService(AppDbContext db, IArquivoService arquivos)
    {
        _db = db;
        _arquivos = arquivos;
    }

    private static readonly Expression<Func<GrupoCultural, GrupoCulturalDto>> ToDto =
        g => new GrupoCulturalDto
        {
            Id = g.Id,
            Nome = g.Nome,
            Descricao = g.Descricao,
            ImagemArquivoId = g.ImagemArquivoId,
            Ordem = g.Ordem,
            Ativo = g.Ativo
        };

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

    public async Task SalvarAsync(GrupoCulturalDto dto, IFormFile? imagem = null, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            var novo = new GrupoCultural
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Ordem = dto.Ordem,
                Ativo = true
            };

            if (imagem is { Length: > 0 })
                novo.ImagemArquivoId = await _arquivos.SalvarAsync(imagem, ct);

            _db.GruposCulturais.Add(novo);
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            var entidade = await _db.GruposCulturais.FirstOrDefaultAsync(g => g.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Grupo cultural não encontrado.");

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

            // Remove o arquivo antigo só após o commit (senão a checagem de
            // referência no banco impediria a exclusão).
            if (antigoId.HasValue)
                await _arquivos.ExcluirAsync(antigoId.Value, ct);
        }
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.GruposCulturais.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new InvalidOperationException("Grupo cultural não encontrado.");
        var imagemId = entidade.ImagemArquivoId;
        _db.GruposCulturais.Remove(entidade);
        await _db.SaveChangesAsync(ct);
        if (imagemId.HasValue)
            await _arquivos.ExcluirAsync(imagemId.Value, ct);
    }
}
