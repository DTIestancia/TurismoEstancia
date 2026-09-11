using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Infra.Interfaces;
using TurismoEstancia.Services.Planeje.Interfaces;
using PlanejeAvaliacaoEntity = TurismoEstancia.Domain.Models.PlanejeAvaliacao;
using PlanejeCategoriaEntity = TurismoEstancia.Domain.Models.PlanejeCategoria;
using PlanejeItemEntity = TurismoEstancia.Domain.Models.PlanejeItem;

namespace TurismoEstancia.Services.Planeje.Services;

/// <summary>Implementação do serviço do "Planeje sua viagem".</summary>
public class PlanejeService : IPlanejeService
{
    private readonly AppDbContext _db;
    private readonly IArquivoService _arquivos;

    public PlanejeService(AppDbContext db, IArquivoService arquivos)
    {
        _db = db;
        _arquivos = arquivos;
    }

    private static readonly Expression<Func<PlanejeCategoriaEntity, PlanejeCategoriaDto>> ToCategoriaDto =
        c => new PlanejeCategoriaDto
        {
            Id = c.Id,
            Nome = c.Nome,
            Descricao = c.Descricao,
            Icone = c.Icone,
            Cor = c.Cor,
            ImagemPadraoArquivoId = c.ImagemPadraoArquivoId,
            Ordem = c.Ordem,
            Ativo = c.Ativo
        };

    private static readonly Expression<Func<PlanejeItemEntity, PlanejeItemDto>> ToItemDto =
        i => new PlanejeItemDto
        {
            Id = i.Id,
            CategoriaId = i.CategoriaId,
            CategoriaNome = i.Categoria != null ? i.Categoria.Nome : null,
            CategoriaIcone = i.Categoria != null ? i.Categoria.Icone : null,
            CategoriaCor = i.Categoria != null ? i.Categoria.Cor : null,
            CategoriaImagemPadraoArquivoId = i.Categoria != null ? i.Categoria.ImagemPadraoArquivoId : null,
            Titulo = i.Titulo,
            Descricao = i.Descricao,
            Localizacao = i.Localizacao,
            Contato = i.Contato,
            Site = i.Site,
            Instagram = i.Instagram,
            ImagemArquivoId = i.ImagemArquivoId,
            Ordem = i.Ordem,
            Ativo = i.Ativo
        };

    private static readonly Expression<Func<PlanejeAvaliacaoEntity, PlanejeAvaliacaoDto>> ToAvaliacaoDto =
        a => new PlanejeAvaliacaoDto
        {
            Id = a.Id,
            PlanejeItemId = a.PlanejeItemId,
            PlanejeItemTitulo = a.PlanejeItem != null ? a.PlanejeItem.Titulo : null,
            Nome = a.Nome,
            Nota = a.Nota,
            Comentario = a.Comentario,
            Data = a.Data,
            Aprovada = a.Aprovada
        };

    public async Task<IReadOnlyList<PlanejeCategoriaDto>> ListarCategoriasAsync(bool apenasAtivas = true, CancellationToken ct = default) =>
        await _db.PlanejeCategorias.AsNoTracking()
            .Where(c => !apenasAtivas || c.Ativo)
            .OrderBy(c => c.Ordem)
            .Select(ToCategoriaDto)
            .ToListAsync(ct);

    public async Task<PlanejeCategoriaDto?> ObterCategoriaAsync(int id, CancellationToken ct = default) =>
        await _db.PlanejeCategorias.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(ToCategoriaDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarCategoriaAsync(PlanejeCategoriaDto dto, IFormFile? imagemPadrao, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            var novo = new PlanejeCategoriaEntity
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Icone = dto.Icone,
                Cor = dto.Cor,
                Ordem = dto.Ordem,
                Ativo = true
            };
            if (imagemPadrao is { Length: > 0 })
                novo.ImagemPadraoArquivoId = await _arquivos.SalvarAsync(imagemPadrao, ct);
            _db.PlanejeCategorias.Add(novo);
            await _db.SaveChangesAsync(ct);
            return;
        }

        var entidade = await _db.PlanejeCategorias.FirstOrDefaultAsync(c => c.Id == dto.Id, ct)
            ?? throw new InvalidOperationException("Categoria não encontrada.");
        entidade.Nome = dto.Nome;
        entidade.Descricao = dto.Descricao;
        entidade.Icone = dto.Icone;
        entidade.Cor = dto.Cor;
        entidade.Ordem = dto.Ordem;

        long? antigoId = null;
        if (imagemPadrao is { Length: > 0 })
        {
            antigoId = entidade.ImagemPadraoArquivoId;
            entidade.ImagemPadraoArquivoId = await _arquivos.SalvarAsync(imagemPadrao, ct);
        }
        await _db.SaveChangesAsync(ct);
        if (antigoId.HasValue)
            await _arquivos.ExcluirAsync(antigoId.Value, ct);
    }

    public async Task ExcluirCategoriaAsync(int id, CancellationToken ct = default)
    {
        if (await _db.PlanejeItens.AnyAsync(i => i.CategoriaId == id, ct))
            throw new InvalidOperationException("Categoria possui itens vinculados.");
        var entidade = await _db.PlanejeCategorias.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new InvalidOperationException("Categoria não encontrada.");
        var imagemId = entidade.ImagemPadraoArquivoId;
        _db.PlanejeCategorias.Remove(entidade);
        await _db.SaveChangesAsync(ct);
        if (imagemId.HasValue)
            await _arquivos.ExcluirAsync(imagemId.Value, ct);
    }

    public async Task OcultarCategoriaAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.PlanejeCategorias.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new InvalidOperationException("Categoria não encontrada.");
        entidade.Ativo = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ReativarCategoriaAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.PlanejeCategorias.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new InvalidOperationException("Categoria não encontrada.");
        entidade.Ativo = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<PlanejeItemDto>> ListarItensAsync(bool apenasAtivos = true, CancellationToken ct = default) =>
        await _db.PlanejeItens.AsNoTracking()
            .Include(i => i.Categoria)
            .Where(i => !apenasAtivos || (i.Ativo && (i.Categoria == null || i.Categoria.Ativo)))
            .OrderBy(i => i.Categoria!.Ordem)
            .ThenBy(i => i.Ordem)
            .Select(ToItemDto)
            .ToListAsync(ct);

    public async Task<PlanejeItemDto?> ObterItemAsync(int id, CancellationToken ct = default) =>
        await _db.PlanejeItens.AsNoTracking()
            .Include(i => i.Categoria)
            .Where(i => i.Id == id)
            .Select(ToItemDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarItemAsync(PlanejeItemDto dto, IFormFile? imagem, CancellationToken ct = default)
    {
        if (!await _db.PlanejeCategorias.AnyAsync(c => c.Id == dto.CategoriaId, ct))
            throw new InvalidOperationException("Categoria inválida.");

        if (dto.Id == 0)
        {
            var novo = new PlanejeItemEntity
            {
                CategoriaId = dto.CategoriaId,
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Localizacao = dto.Localizacao,
                Contato = dto.Contato,
                Site = dto.Site,
                Instagram = dto.Instagram,
                Ordem = dto.Ordem,
                Ativo = dto.Ativo
            };
            if (imagem is { Length: > 0 })
                novo.ImagemArquivoId = await _arquivos.SalvarAsync(imagem, ct);
            _db.PlanejeItens.Add(novo);
            await _db.SaveChangesAsync(ct);
            return;
        }

        var entidade = await _db.PlanejeItens.FirstOrDefaultAsync(i => i.Id == dto.Id, ct)
            ?? throw new InvalidOperationException("Item não encontrado.");
        entidade.CategoriaId = dto.CategoriaId;
        entidade.Titulo = dto.Titulo;
        entidade.Descricao = dto.Descricao;
        entidade.Localizacao = dto.Localizacao;
        entidade.Contato = dto.Contato;
        entidade.Site = dto.Site;
        entidade.Instagram = dto.Instagram;
        entidade.Ordem = dto.Ordem;
        entidade.Ativo = dto.Ativo;

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

    public async Task ExcluirItemAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.PlanejeItens.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new InvalidOperationException("Item não encontrado.");
        var imagemId = entidade.ImagemArquivoId;
        _db.PlanejeItens.Remove(entidade);
        await _db.SaveChangesAsync(ct);
        if (imagemId.HasValue)
            await _arquivos.ExcluirAsync(imagemId.Value, ct);
    }

    public async Task OcultarItemAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.PlanejeItens.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new InvalidOperationException("Item não encontrado.");
        entidade.Ativo = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ReativarItemAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.PlanejeItens.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new InvalidOperationException("Item não encontrado.");
        entidade.Ativo = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<PlanejeAvaliacaoDto>> ListarAvaliacoesAsync(int itemId, bool apenasAprovadas = true, CancellationToken ct = default)
    {
        var query = _db.PlanejeAvaliacoes.AsNoTracking().Where(a => a.PlanejeItemId == itemId);
        if (apenasAprovadas)
            query = query.Where(a => a.Aprovada);
        return await query
            .OrderByDescending(a => a.Data)
            .Select(ToAvaliacaoDto)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PlanejeAvaliacaoDto>> ListarTodasAvaliacoesAsync(CancellationToken ct = default) =>
        await _db.PlanejeAvaliacoes.AsNoTracking()
            .Include(a => a.PlanejeItem)
            .OrderByDescending(a => a.Data)
            .Select(ToAvaliacaoDto)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PlanejeAvaliacaoDto>> ListarPendentesAsync(CancellationToken ct = default) =>
        await _db.PlanejeAvaliacoes.AsNoTracking()
            .Include(a => a.PlanejeItem)
            .Where(a => !a.Aprovada)
            .OrderByDescending(a => a.Data)
            .Select(ToAvaliacaoDto)
            .ToListAsync(ct);

    public async Task SubmeterAvaliacaoAsync(PlanejeAvaliacaoDto dto, CancellationToken ct = default)
    {
        if (dto.Nota < 1 || dto.Nota > 5)
            throw new InvalidOperationException("A nota deve estar entre 1 e 5.");
        if (string.IsNullOrWhiteSpace(dto.Comentario) || dto.Comentario.Trim().Length < 50)
            throw new InvalidOperationException("Conte sua experiência com ao menos 50 caracteres.");
        if (!await _db.PlanejeItens.AnyAsync(i => i.Id == dto.PlanejeItemId, ct))
            throw new InvalidOperationException("Item não encontrado.");

        _db.PlanejeAvaliacoes.Add(new PlanejeAvaliacaoEntity
        {
            PlanejeItemId = dto.PlanejeItemId,
            Nome = string.IsNullOrWhiteSpace(dto.Nome) ? null : dto.Nome.Trim(),
            Nota = dto.Nota,
            Comentario = dto.Comentario.Trim(),
            Aprovada = false // sempre entra para moderação
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task AprovarAvaliacaoAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.PlanejeAvaliacoes.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new InvalidOperationException("Avaliação não encontrada.");
        entidade.Aprovada = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAvaliacaoAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.PlanejeAvaliacoes.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new InvalidOperationException("Avaliação não encontrada.");
        _db.PlanejeAvaliacoes.Remove(entidade);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> ContarPendentesAsync(CancellationToken ct = default) =>
        await _db.PlanejeAvaliacoes.CountAsync(a => !a.Aprovada, ct);
}
