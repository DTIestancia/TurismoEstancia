using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.ConhecaEstancia.Interfaces;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Services.ConhecaEstancia.Services;

/// <summary>Implementação do serviço da seção "Conheça Estância".</summary>
public class ConhecaEstanciaService : IConhecaEstanciaService
{
    private readonly AppDbContext _db;
    private readonly IArquivoService _arquivos;

    public ConhecaEstanciaService(AppDbContext db, IArquivoService arquivos)
    {
        _db = db;
        _arquivos = arquivos;
    }

    private static readonly Expression<Func<ConhecaEstanciaItem, ConhecaEstanciaItemDto>> ToDto =
        i => new ConhecaEstanciaItemDto
        {
            Id = i.Id,
            Categoria = i.Categoria,
            Nome = i.Nome,
            Descricao = i.Descricao,
            ImagemArquivoId = i.ImagemArquivoId,
            Ordem = i.Ordem,
            Ativo = i.Ativo
        };

    public async Task<IReadOnlyList<ConhecaEstanciaItemDto>> ListarAsync(CancellationToken ct = default) =>
        await _db.ConhecaEstanciaItens.AsNoTracking()
            .OrderBy(i => i.Categoria)
            .ThenBy(i => i.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ConhecaEstanciaItemDto>> ListarAtivosAsync(CancellationToken ct = default) =>
        await _db.ConhecaEstanciaItens.AsNoTracking()
            .Where(i => i.Ativo)
            .OrderBy(i => i.Categoria)
            .ThenBy(i => i.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<ConhecaEstanciaItemDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.ConhecaEstanciaItens.AsNoTracking()
            .Where(i => i.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarAsync(ConhecaEstanciaItemDto dto, IFormFile? imagem = null, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            var novo = new ConhecaEstanciaItem
            {
                Categoria = dto.Categoria,
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Ordem = dto.Ordem,
                Ativo = dto.Ativo
            };

            if (imagem is { Length: > 0 })
                novo.ImagemArquivoId = await _arquivos.SalvarAsync(imagem, ct);

            _db.ConhecaEstanciaItens.Add(novo);
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            var entidade = await _db.ConhecaEstanciaItens.FirstOrDefaultAsync(i => i.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Item do Conheça Estância não encontrado.");

            entidade.Categoria = dto.Categoria;
            entidade.Nome = dto.Nome;
            entidade.Descricao = dto.Descricao;
            entidade.Ordem = dto.Ordem;
            entidade.Ativo = dto.Ativo;

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
        var entidade = await _db.ConhecaEstanciaItens.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new InvalidOperationException("Item do Conheça Estância não encontrado.");
        var imagemId = entidade.ImagemArquivoId;
        _db.ConhecaEstanciaItens.Remove(entidade);
        await _db.SaveChangesAsync(ct);
        if (imagemId.HasValue)
            await _arquivos.ExcluirAsync(imagemId.Value, ct);
    }
}
