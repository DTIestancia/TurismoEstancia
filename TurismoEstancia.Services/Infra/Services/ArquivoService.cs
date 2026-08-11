using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
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

    public async Task<long> SalvarImagemOtimizadaAsync(IFormFile arquivo, int maxDimensao = 1600, int qualidade = 82, bool comMarcaDagua = false, CancellationToken ct = default)
    {
        using var imagem = await CarregarImagemAsync(arquivo, ct);
        if (comMarcaDagua)
            await AplicarMarcaDaguaAsync(imagem, ct);
        return await SalvarImagemCoreAsync(imagem, arquivo.FileName, maxDimensao, qualidade, ct);
    }

    public async Task<long> SalvarThumbnailAsync(IFormFile arquivo, int maxDimensao = 400, int qualidade = 75, CancellationToken ct = default)
    {
        using var imagem = await CarregarImagemAsync(arquivo, ct);
        return await SalvarImagemCoreAsync(imagem, arquivo.FileName, maxDimensao, qualidade, ct);
    }

    /// <summary>
    /// Marca d'água de proteção contra download: listras diagonais sutis por toda
    /// a imagem + o logotipo do portal (configuração "logo-principal") no canto
    /// inferior direito. Só usa o core do ImageSharp (sem dependência extra).
    /// Se o logotipo não existir/falhar, aplica só as listras — nunca quebra o upload.
    /// </summary>
    private async Task AplicarMarcaDaguaAsync(Image imagem, CancellationToken ct)
    {
        try
        {
            // Listras diagonais: padrão em baixa resolução + resize bilinear
            // (suaviza as bordas) + composição com alfa baixo.
            var pw = Math.Max(64, imagem.Width / 8);
            var ph = Math.Max(48, imagem.Height / 8);
            using (var padrao = new Image<Rgba32>(pw, ph))
            {
                for (var y = 0; y < ph; y++)
                {
                    for (var x = 0; x < pw; x++)
                    {
                        padrao[x, y] = (x + y) % 32 < 16
                            ? new Rgba32(255, 255, 255, 26)
                            : new Rgba32(255, 255, 255, 0);
                    }
                }

                padrao.Mutate(p => p.Resize(imagem.Width, imagem.Height));
                imagem.Mutate(m => m.DrawImage(padrao, 1f));
            }

            // Logotipo do portal no canto inferior direito (marca real do município).
            var logoBytes = await _db.ConfiguracoesSite.AsNoTracking()
                .Where(c => c.Chave == "logo-principal" && c.ArquivoId != null)
                .Select(c => c.Arquivo!.Bytes)
                .FirstOrDefaultAsync(ct);

            if (logoBytes is { Length: > 0 })
            {
                using var logo = await Image.LoadAsync(new MemoryStream(logoBytes), ct);
                var larguraLogo = Math.Min(150, imagem.Width / 4);
                var alturaLogo = Math.Max(24, (int)(logo.Height * (larguraLogo / (float)logo.Width)));
                logo.Mutate(l => l.Resize(larguraLogo, alturaLogo));

                var x = imagem.Width - larguraLogo - 16;
                var y = imagem.Height - alturaLogo - 16;
                imagem.Mutate(m => m.DrawImage(logo, new Point(x, y), 0.85f));
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Marca d'água é cosmética — jamais deve derrubar o upload.
        }
    }

    /// <summary>
    /// Carrega e valida o upload como imagem (JPG, PNG ou WebP). A validação por
    /// extensão é intencionalmente ignorada — só o decode real confirma o formato.
    /// </summary>
    private static async Task<Image> CarregarImagemAsync(IFormFile arquivo, CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
            throw new InvalidOperationException("O arquivo está vazio.");

        try
        {
            await using var stream = arquivo.OpenReadStream();
            return await Image.LoadAsync(stream, ct);
        }
        catch (UnknownImageFormatException)
        {
            throw new InvalidOperationException("Formato não suportado: envie uma imagem JPG, PNG ou WebP.");
        }
    }

    /// <summary>
    /// Redimensiona (só reduz, nunca amplia), re-encoda como JPEG com SkipMetadata
    /// (remove EXIF/GPS — privacidade LGPD) e grava na tabela Arquivo.
    /// </summary>
    private async Task<long> SalvarImagemCoreAsync(Image imagem, string nome, int maxDimensao, int qualidade, CancellationToken ct)
    {
        if (imagem.Width > maxDimensao || imagem.Height > maxDimensao)
        {
            imagem.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxDimensao, maxDimensao)
            }));
        }

        using var ms = new MemoryStream();
        await imagem.SaveAsync(ms, new JpegEncoder { Quality = qualidade, SkipMetadata = true }, ct);
        return await SalvarBytesAsync(nome, "image/jpeg", ms.ToArray(), ct);
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
        if (await _db.GaleriaMidias.AnyAsync(m => m.ArquivoId == id || m.ArquivoThumbId == id, ct)) return true;
        if (await _db.GaleriaCategorias.AnyAsync(c => c.CapaArquivoId == id, ct)) return true;
        return false;
    }
}
