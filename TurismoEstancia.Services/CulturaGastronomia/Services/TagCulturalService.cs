using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Services.CulturaGastronomia.Services;

/// <summary>Implementação do serviço de tags culturais.</summary>
public class TagCulturalService : ITagCulturalService
{
    private readonly AppDbContext _db;
    private readonly IArquivoService _arquivos;

    public TagCulturalService(AppDbContext db, IArquivoService arquivos)
    {
        _db = db;
        _arquivos = arquivos;
    }

    private static readonly Expression<Func<TagCultural, TagCulturalDto>> ToDto =
        t => new TagCulturalDto
        {
            Id = t.Id,
            Nome = t.Nome,
            Descricao = t.Descricao,
            ImagemArquivoId = t.ImagemArquivoId,
            Ordem = t.Ordem,
            Ativo = t.Ativo
        };

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

    public async Task SalvarAsync(TagCulturalDto dto, IFormFile? imagem = null, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            var novo = new TagCultural
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Ordem = dto.Ordem,
                Ativo = true
            };

            if (imagem is { Length: > 0 })
                novo.ImagemArquivoId = await _arquivos.SalvarAsync(imagem, ct);

            _db.TagsCulturais.Add(novo);
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            var entidade = await _db.TagsCulturais.FirstOrDefaultAsync(t => t.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Tag cultural não encontrada.");

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
        var entidade = await _db.TagsCulturais.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new InvalidOperationException("Tag cultural não encontrada.");
        var imagemId = entidade.ImagemArquivoId;
        _db.TagsCulturais.Remove(entidade);
        await _db.SaveChangesAsync(ct);
        if (imagemId.HasValue)
            await _arquivos.ExcluirAsync(imagemId.Value, ct);
    }
}
