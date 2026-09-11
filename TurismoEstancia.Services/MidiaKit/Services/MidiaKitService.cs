using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Infra.Interfaces;
using TurismoEstancia.Services.MidiaKit.Interfaces;

namespace TurismoEstancia.Services.MidiaKit.Services;

/// <summary>Implementação do serviço do Mídia kit.</summary>
public class MidiaKitService : IMidiaKitService
{
    private readonly AppDbContext _db;
    private readonly IArquivoService _arquivos;

    public MidiaKitService(AppDbContext db, IArquivoService arquivos)
    {
        _db = db;
        _arquivos = arquivos;
    }

    private static readonly Expression<Func<MidiaKitItem, MidiaKitItemDto>> ToDto =
        i => new MidiaKitItemDto
        {
            Id = i.Id,
            Titulo = i.Titulo,
            Descricao = i.Descricao,
            ArquivoId = i.ArquivoId,
            ArquivoNome = i.Arquivo != null ? i.Arquivo.Nome : null,
            ArquivoContentType = i.Arquivo != null ? i.Arquivo.ContentType : null,
            ArquivoSize = i.Arquivo != null ? (long?)i.Arquivo.Size : null,
            Ordem = i.Ordem,
            Ativo = i.Ativo,
            Data = i.Data
        };

    public async Task<IReadOnlyList<MidiaKitItemDto>> ListarAsync(CancellationToken ct = default) =>
        await _db.MidiaKitItens.AsNoTracking()
            .Include(i => i.Arquivo)
            .OrderBy(i => i.Ordem)
            .ThenByDescending(i => i.Data)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MidiaKitItemDto>> ListarAtivosAsync(CancellationToken ct = default) =>
        await _db.MidiaKitItens.AsNoTracking()
            .Include(i => i.Arquivo)
            .Where(i => i.Ativo && i.ArquivoId != null)
            .OrderBy(i => i.Ordem)
            .ThenByDescending(i => i.Data)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<MidiaKitItemDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.MidiaKitItens.AsNoTracking()
            .Include(i => i.Arquivo)
            .Where(i => i.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarAsync(MidiaKitItemDto dto, IFormFile? arquivo = null, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            var novo = new MidiaKitItem
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Ordem = dto.Ordem,
                Ativo = dto.Ativo
            };

            if (arquivo is { Length: > 0 })
                novo.ArquivoId = await _arquivos.SalvarAsync(arquivo, ct);

            _db.MidiaKitItens.Add(novo);
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            var entidade = await _db.MidiaKitItens.FirstOrDefaultAsync(i => i.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Item do mídia kit não encontrado.");

            entidade.Titulo = dto.Titulo;
            entidade.Descricao = dto.Descricao;
            entidade.Ordem = dto.Ordem;
            entidade.Ativo = dto.Ativo;

            long? antigoId = null;
            if (arquivo is { Length: > 0 })
            {
                antigoId = entidade.ArquivoId;
                entidade.ArquivoId = await _arquivos.SalvarAsync(arquivo, ct);
            }

            await _db.SaveChangesAsync(ct);

            // Remove o arquivo antigo só após o commit (senão a checagem de
            // referência no banco impediria a exclusão).
            if (antigoId.HasValue)
                await _arquivos.ExcluirAsync(antigoId.Value, ct);
        }
    }

    public async Task OcultarAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.MidiaKitItens.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new InvalidOperationException("Item do mídia kit não encontrado.");
        entidade.Ativo = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ReativarAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.MidiaKitItens.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new InvalidOperationException("Item do mídia kit não encontrado.");
        entidade.Ativo = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.MidiaKitItens.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new InvalidOperationException("Item do mídia kit não encontrado.");
        var arquivoId = entidade.ArquivoId;
        _db.MidiaKitItens.Remove(entidade);
        await _db.SaveChangesAsync(ct);
        if (arquivoId.HasValue)
            await _arquivos.ExcluirAsync(arquivoId.Value, ct);
    }
}
