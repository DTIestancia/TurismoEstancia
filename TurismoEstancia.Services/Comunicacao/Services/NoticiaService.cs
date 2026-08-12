using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Services.Comunicacao.Services;

/// <summary>Implementação do serviço de notícias.</summary>
public class NoticiaService : INoticiaService
{
    private readonly AppDbContext _db;
    private readonly IArquivoService _arquivos;

    public NoticiaService(AppDbContext db, IArquivoService arquivos)
    {
        _db = db;
        _arquivos = arquivos;
    }

    private static readonly Expression<Func<Noticia, NoticiaDto>> ToDto =
        n => new NoticiaDto
        {
            Id = n.Id,
            Titulo = n.Titulo,
            Resumo = n.Resumo,
            Corpo = n.Corpo,
            ImagemArquivoId = n.ImagemArquivoId,
            GaleriaCategoriaId = n.GaleriaCategoriaId,
            GaleriaNome = n.Galeria != null ? n.Galeria.Nome : null,
            DataPublicacao = n.DataPublicacao,
            Slug = n.Slug,
            Publicada = n.Publicada,
            Ordem = n.Ordem,
            Ativo = n.Ativo
        };

    public async Task<IReadOnlyList<NoticiaDto>> ListarAsync(bool apenasPublicadas = false, CancellationToken ct = default)
    {
        var query = _db.Noticias.AsNoTracking().Where(n => n.Ativo);
        if (apenasPublicadas)
            query = query.Where(n => n.Publicada);

        return await query
            .OrderByDescending(n => n.DataPublicacao)
            .ThenBy(n => n.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);
    }

    public async Task<NoticiaDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.Noticias.AsNoTracking()
            .Where(n => n.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task<NoticiaDto?> ObterPorSlugAsync(string slug, CancellationToken ct = default) =>
        await _db.Noticias.AsNoTracking()
            .Where(n => n.Slug == slug && n.Ativo && n.Publicada)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarAsync(NoticiaDto dto, IFormFile? imagem, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug))
            dto.Slug = await GerarSlugAsync(dto.Titulo, dto.Id == 0 ? null : dto.Id, ct);
        else
            dto.Slug = dto.Slug.Trim().ToLowerInvariant();

        var arquivosParaExcluir = new List<long>();

        if (dto.Id == 0)
        {
            if (await _db.Noticias.AnyAsync(n => n.Slug == dto.Slug, ct))
                throw new InvalidOperationException("Já existe uma notícia com este slug.");

            var nova = new Noticia
            {
                Titulo = dto.Titulo,
                Resumo = dto.Resumo,
                Corpo = dto.Corpo,
                GaleriaCategoriaId = dto.GaleriaCategoriaId,
                DataPublicacao = dto.DataPublicacao == default ? DateTime.Now : dto.DataPublicacao,
                Slug = dto.Slug,
                Publicada = dto.Publicada,
                Ordem = dto.Ordem,
                Ativo = true
            };

            if (imagem is { Length: > 0 })
                nova.ImagemArquivoId = await _arquivos.SalvarAsync(imagem, ct);

            _db.Noticias.Add(nova);
        }
        else
        {
            var entidade = await _db.Noticias.FirstOrDefaultAsync(n => n.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Notícia não encontrada.");

            if (await _db.Noticias.AnyAsync(n => n.Slug == dto.Slug && n.Id != dto.Id, ct))
                throw new InvalidOperationException("Já existe uma notícia com este slug.");

            entidade.Titulo = dto.Titulo;
            entidade.Resumo = dto.Resumo;
            entidade.Corpo = dto.Corpo;
            entidade.GaleriaCategoriaId = dto.GaleriaCategoriaId;
            entidade.Slug = dto.Slug;
            entidade.Publicada = dto.Publicada;
            entidade.Ordem = dto.Ordem;

            if (imagem is { Length: > 0 })
            {
                var antigoId = entidade.ImagemArquivoId;
                entidade.ImagemArquivoId = await _arquivos.SalvarAsync(imagem, ct);
                arquivosParaExcluir.Add(antigoId ?? 0);
            }
        }

        await _db.SaveChangesAsync(ct);

        // Remove arquivos antigos só após o commit.
        foreach (var id in arquivosParaExcluir.Where(i => i > 0))
            await _arquivos.ExcluirAsync(id, ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.Noticias.FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new InvalidOperationException("Notícia não encontrada.");
        entidade.Ativo = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<string> GerarSlugAsync(string titulo, int? ignorarId = null, CancellationToken ct = default)
    {
        var baseSlug = NormalizarSlug(titulo);
        var slug = baseSlug;
        var contador = 2;

        while (await _db.Noticias.AnyAsync(n => n.Slug == slug && (!ignorarId.HasValue || n.Id != ignorarId.Value), ct))
        {
            slug = $"{baseSlug}-{contador}";
            contador++;
        }

        return slug;
    }

    private static string NormalizarSlug(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo)) return "noticia";

        var normalizado = titulo.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"[\s_-]+", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "noticia" : slug;
    }
}
