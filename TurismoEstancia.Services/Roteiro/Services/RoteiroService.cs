using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Infra.Interfaces;
using TurismoEstancia.Services.Roteiro.Interfaces;
using RoteiroEntity = TurismoEstancia.Domain.Models.Roteiro;

namespace TurismoEstancia.Services.Roteiro.Services;

/// <summary>Implementação do serviço de roteiros.</summary>
public class RoteiroService : IRoteiroService
{
    private readonly AppDbContext _db;
    private readonly IArquivoService _arquivos;

    public RoteiroService(AppDbContext db, IArquivoService arquivos)
    {
        _db = db;
        _arquivos = arquivos;
    }

    private static readonly Expression<Func<RoteiroEntity, RoteiroDto>> ToDto =
        r => new RoteiroDto
        {
            Id = r.Id,
            Titulo = r.Titulo,
            Descricao = r.Descricao,
            ImagemArquivoId = r.ImagemArquivoId,
            Ordem = r.Ordem,
            Ativo = r.Ativo
        };

    public async Task<IReadOnlyList<RoteiroDto>> ListarAsync(CancellationToken ct = default)
    {
        var roteiros = await _db.Roteiros.AsNoTracking()
            .Where(r => r.Ativo)
            .OrderBy(r => r.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);

        await CarregarItensAsync(roteiros, ct);
        return roteiros;
    }

    public async Task<RoteiroDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _db.Roteiros.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

        if (dto is null) return null;
        await CarregarItensAsync(new[] { dto }, ct);
        return dto;
    }

    public async Task SalvarAsync(RoteiroDto dto, IFormFile? imagem, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            var novo = new RoteiroEntity
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Ordem = dto.Ordem,
                Ativo = true
            };

            if (imagem is { Length: > 0 })
                novo.ImagemArquivoId = await _arquivos.SalvarAsync(imagem, ct);

            _db.Roteiros.Add(novo);
            await _db.SaveChangesAsync(ct);
            dto.Id = novo.Id;

            await SincronizarItensAsync(novo.Id, dto.Itens, ct);
        }
        else
        {
            var entidade = await _db.Roteiros.FirstOrDefaultAsync(r => r.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Roteiro não encontrado.");

            entidade.Titulo = dto.Titulo;
            entidade.Descricao = dto.Descricao;
            entidade.Ordem = dto.Ordem;

            long? antigoId = null;
            if (imagem is { Length: > 0 })
            {
                antigoId = entidade.ImagemArquivoId;
                entidade.ImagemArquivoId = await _arquivos.SalvarAsync(imagem, ct);
            }

            await SincronizarItensAsync(dto.Id, dto.Itens, ct);
            await _db.SaveChangesAsync(ct);

            // Remove o arquivo antigo só após o commit.
            if (antigoId.HasValue)
                await _arquivos.ExcluirAsync(antigoId.Value, ct);
            return;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.Roteiros.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new InvalidOperationException("Roteiro não encontrado.");
        var imagemId = entidade.ImagemArquivoId;
        _db.Roteiros.Remove(entidade);
        await _db.SaveChangesAsync(ct);
        if (imagemId.HasValue)
            await _arquivos.ExcluirAsync(imagemId.Value, ct);
    }

    private async Task CarregarItensAsync(IEnumerable<RoteiroDto> roteiros, CancellationToken ct)
    {
        var ids = roteiros.Select(r => r.Id).ToList();
        if (ids.Count == 0) return;

        var itens = await _db.RoteiroItens.AsNoTracking()
            .Include(i => i.PontoTuristico)
            .Where(i => ids.Contains(i.RoteiroId))
            .OrderBy(i => i.Dia)
            .ThenBy(i => i.Ordem)
            .ToListAsync(ct);

        foreach (var roteiro in roteiros)
        {
            roteiro.Itens = itens
                .Where(i => i.RoteiroId == roteiro.Id)
                .Select(i => new RoteiroItemDto
                {
                    Id = i.Id,
                    RoteiroId = i.RoteiroId,
                    PontoTuristicoId = i.PontoTuristicoId,
                    PontoTuristicoNome = i.PontoTuristico != null ? i.PontoTuristico.Nome : null,
                    Dia = i.Dia,
                    Ordem = i.Ordem,
                    Observacao = i.Observacao
                })
                .ToList();
        }
    }

    private async Task SincronizarItensAsync(int roteiroId, List<RoteiroItemDto> itens, CancellationToken ct)
    {
        var atuais = await _db.RoteiroItens
            .Where(i => i.RoteiroId == roteiroId)
            .ToListAsync(ct);

        var idsRecebidos = itens.Select(i => i.Id).ToHashSet();
        foreach (var atual in atuais.Where(i => !idsRecebidos.Contains(i.Id)))
            _db.RoteiroItens.Remove(atual);

        foreach (var item in itens)
        {
            if (item.Id > 0)
            {
                var existente = atuais.FirstOrDefault(i => i.Id == item.Id);
                if (existente is not null)
                {
                    existente.PontoTuristicoId = item.PontoTuristicoId;
                    existente.Dia = item.Dia;
                    existente.Ordem = item.Ordem;
                    existente.Observacao = item.Observacao;
                }
            }
            else
            {
                _db.RoteiroItens.Add(new RoteiroItem
                {
                    RoteiroId = roteiroId,
                    PontoTuristicoId = item.PontoTuristicoId,
                    Dia = item.Dia,
                    Ordem = item.Ordem,
                    Observacao = item.Observacao
                });
            }
        }
    }
}
