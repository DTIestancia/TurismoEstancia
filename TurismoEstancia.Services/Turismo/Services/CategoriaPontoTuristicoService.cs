using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Services.Turismo.Services;

/// <summary>Implementação do serviço de categoria de ponto turístico.</summary>
public class CategoriaPontoTuristicoService : ICategoriaPontoTuristicoService
{
    private readonly AppDbContext _db;

    public CategoriaPontoTuristicoService(AppDbContext db) => _db = db;

    private static readonly Expression<Func<CategoriaPontoTuristico, CategoriaPontoTuristicoDto>> ToDto =
        c => new CategoriaPontoTuristicoDto
        {
            Id = c.Id,
            Chave = c.Chave,
            Nome = c.Nome,
            SubTitulo = c.SubTitulo,
            Cor = c.Cor,
            Icone = c.Icone,
            ApresentarEmMaravilhas = c.ApresentarEmMaravilhas,
            ExibirNoMapa = c.ExibirNoMapa,
            Ordem = c.Ordem,
            Ativo = c.Ativo,
            QuantidadePontos = c.PontosTuristicos.Count(p => p.Ativo)
        };

    public async Task<IReadOnlyList<CategoriaPontoTuristicoDto>> ListarAsync(bool incluirInativos = false, CancellationToken ct = default) =>
        await _db.CategoriasPontosTuristicos.AsNoTracking()
            .Where(c => incluirInativos || c.Ativo)
            .OrderBy(c => c.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<CategoriaPontoTuristicoDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.CategoriasPontosTuristicos.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarAsync(CategoriaPontoTuristicoDto dto, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            _db.CategoriasPontosTuristicos.Add(new CategoriaPontoTuristico
            {
                Chave = dto.Chave,
                Nome = dto.Nome,
                SubTitulo = dto.SubTitulo,
                Cor = dto.Cor,
                Icone = dto.Icone,
                ApresentarEmMaravilhas = dto.ApresentarEmMaravilhas,
                ExibirNoMapa = dto.ExibirNoMapa,
                Ordem = dto.Ordem,
                Ativo = true
            });
        }
        else
        {
            var entidade = await _db.CategoriasPontosTuristicos.FirstOrDefaultAsync(c => c.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Categoria não encontrada.");
            entidade.Chave = dto.Chave;
            entidade.Nome = dto.Nome;
            entidade.SubTitulo = dto.SubTitulo;
            entidade.Cor = dto.Cor;
            entidade.Icone = dto.Icone;
            entidade.ApresentarEmMaravilhas = dto.ApresentarEmMaravilhas;
            entidade.ExibirNoMapa = dto.ExibirNoMapa;
            entidade.Ordem = dto.Ordem;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.CategoriasPontosTuristicos.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new InvalidOperationException("Categoria não encontrada.");

        if (await _db.PontosTuristicos.AnyAsync(p => p.CategoriaId == id && p.Ativo, ct))
            throw new InvalidOperationException("Não é possível excluir: existem pontos turísticos ativos nesta categoria.");

        _db.CategoriasPontosTuristicos.Remove(entidade);
        await _db.SaveChangesAsync(ct);
    }
}
