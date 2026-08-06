using Microsoft.EntityFrameworkCore;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Services.Infra.Services;

/// <summary>Implementação do serviço de Arquivo.</summary>
public class ArquivoService : IArquivoService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public ArquivoService(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task<long> SalvarAsync(IFormFile arquivo, CancellationToken ct = default)
    {
        if (arquivo is null || arquivo.Length == 0)
            throw new InvalidOperationException("O arquivo está vazio.");

        using var ms = new MemoryStream();
        await arquivo.CopyToAsync(ms, ct);
        return await SalvarBytesAsync(arquivo.FileName, arquivo.ContentType, ms.ToArray(), ct);
    }

    public async Task<long> SalvarBytesAsync(string nome, string contentType, byte[] bytes, CancellationToken ct = default)
    {
        var novo = new Arquivo
        {
            UID = Guid.NewGuid(),
            Nome = nome,
            ContentType = contentType,
            Size = bytes.LongLength,
            Bytes = bytes,
            // Padrão Arqu*: autor = usuário logado; origem = canal que gravou.
            Autor = _http.HttpContext?.User?.Identity?.Name,
            Origem = _http.HttpContext is not null ? "cms" : null,
            Ativo = true
        };

        _db.Arquivos.Add(novo);
        await _db.SaveChangesAsync(ct);
        return novo.Id;
    }

    public async Task<Arquivo> ObterAsync(long id, CancellationToken ct = default)
    {
        var arquivo = await _db.Arquivos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new InvalidOperationException("Arquivo não encontrado.");
        return arquivo;
    }

    public async Task ExcluirAsync(long id, CancellationToken ct = default)
    {
        if (await EstaReferenciadoAsync(id, ct))
            return; // nunca apagar arquivo ainda referenciado

        var arquivo = await _db.Arquivos.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (arquivo is not null)
        {
            _db.Arquivos.Remove(arquivo);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> EstaReferenciadoAsync(long id, CancellationToken ct = default)
    {
        if (await _db.Slides.AnyAsync(s => s.ImagemArquivoId == id, ct)) return true;
        if (await _db.PontoTuristicoMidias.AnyAsync(m => m.ArquivoId == id, ct)) return true;
        if (await _db.Noticias.AnyAsync(n => n.ImagemArquivoId == id, ct)) return true;
        if (await _db.Roteiros.AnyAsync(r => r.ImagemArquivoId == id, ct)) return true;
        if (await _db.ConfiguracoesSite.AnyAsync(c => c.ArquivoId == id, ct)) return true;
        return false;
    }
}
