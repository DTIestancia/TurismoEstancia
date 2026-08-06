using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Infra.Interfaces;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Services.Turismo.Services;

/// <summary>Implementação do serviço de slides.</summary>
public class SlideService : ISlideService
{
    private readonly AppDbContext _db;
    private readonly IArquivoService _arquivos;

    public SlideService(AppDbContext db, IArquivoService arquivos)
    {
        _db = db;
        _arquivos = arquivos;
    }

    private static readonly Expression<Func<Slide, SlideDto>> ToDto =
        s => new SlideDto
        {
            Id = s.Id,
            ImagemArquivoId = s.ImagemArquivoId,
            Titulo = s.Titulo,
            Ordem = s.Ordem,
            Ativo = s.Ativo
        };

    public async Task<IReadOnlyList<SlideDto>> ListarAsync(CancellationToken ct = default) =>
        await _db.Slides.AsNoTracking()
            .Where(s => s.Ativo)
            .OrderBy(s => s.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<SlideDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.Slides.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarAsync(SlideDto dto, IFormFile? imagem, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            if (imagem is null || imagem.Length == 0)
                throw new InvalidOperationException("A imagem do slide é obrigatória.");

            var arquivoId = await _arquivos.SalvarAsync(imagem, ct);
            _db.Slides.Add(new Slide
            {
                ImagemArquivoId = arquivoId,
                Titulo = dto.Titulo,
                Ordem = dto.Ordem,
                Ativo = true
            });
        }
        else
        {
            var entidade = await _db.Slides.FirstOrDefaultAsync(s => s.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Slide não encontrado.");

            long? antigoId = null;
            if (imagem is { Length: > 0 })
            {
                antigoId = entidade.ImagemArquivoId;
                entidade.ImagemArquivoId = await _arquivos.SalvarAsync(imagem, ct);
            }

            entidade.Titulo = dto.Titulo;
            entidade.Ordem = dto.Ordem;

            await _db.SaveChangesAsync(ct);

            // Remove o arquivo antigo só após o commit (senão a checagem de referência falha).
            if (antigoId.HasValue)
                await _arquivos.ExcluirAsync(antigoId.Value, ct);
            return;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.Slides.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new InvalidOperationException("Slide não encontrado.");
        var arquivoId = entidade.ImagemArquivoId;
        _db.Slides.Remove(entidade);
        await _db.SaveChangesAsync(ct);
        await _arquivos.ExcluirAsync(arquivoId, ct);
    }
}
