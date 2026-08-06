using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Conteudo.Interfaces;

namespace TurismoEstancia.Services.Conteudo.Services;

/// <summary>Implementação do serviço de contatos do rodapé.</summary>
public class ContatoService : IContatoService
{
    private readonly AppDbContext _db;

    public ContatoService(AppDbContext db) => _db = db;

    private static readonly Expression<Func<Contato, ContatoDto>> ToDto =
        c => new ContatoDto
        {
            Id = c.Id,
            Tipo = c.Tipo,
            Rotulo = c.Rotulo,
            Valor = c.Valor,
            Icone = c.Icone,
            Ordem = c.Ordem,
            Ativo = c.Ativo
        };

    public async Task<IReadOnlyList<ContatoDto>> ListarAsync(TipoContato? tipo = null, CancellationToken ct = default)
    {
        var query = _db.Contatos.AsNoTracking().Where(c => c.Ativo);
        if (tipo.HasValue)
            query = query.Where(c => c.Tipo == tipo.Value);

        return await query.OrderBy(c => c.Ordem).Select(ToDto).ToListAsync(ct);
    }

    public async Task<ContatoDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.Contatos.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarAsync(ContatoDto dto, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            _db.Contatos.Add(new Contato
            {
                Tipo = dto.Tipo,
                Rotulo = dto.Rotulo,
                Valor = dto.Valor,
                Icone = dto.Icone,
                Ordem = dto.Ordem,
                Ativo = true
            });
        }
        else
        {
            var entidade = await _db.Contatos.FirstOrDefaultAsync(c => c.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Contato não encontrado.");
            entidade.Tipo = dto.Tipo;
            entidade.Rotulo = dto.Rotulo;
            entidade.Valor = dto.Valor;
            entidade.Icone = dto.Icone;
            entidade.Ordem = dto.Ordem;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.Contatos.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new InvalidOperationException("Contato não encontrado.");
        _db.Contatos.Remove(entidade);
        await _db.SaveChangesAsync(ct);
    }
}
