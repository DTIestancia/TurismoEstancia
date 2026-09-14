using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Infra.Imagens;
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

        var bytes = await LerBytesAsync(arquivo, ct);

        // Toda foto enviada por qualquer tela do CMS entra otimizada — mesma regra
        // da galeria (ver OtimizadorDeImagem). O que não for foto decodificável
        // (vídeo, PDF, SVG, .ico, GIF animado) segue exatamente como veio.
        var otimizada = await OtimizadorDeImagem.OtimizarAsync(bytes, arquivo.ContentType, ct: ct);

        return otimizada is null
            ? await SalvarBytesAsync(arquivo.FileName, arquivo.ContentType, bytes, ct)
            : await SalvarBytesAsync(
                OtimizadorDeImagem.NomeComExtensao(arquivo.FileName, otimizada.Extensao),
                otimizada.ContentType, otimizada.Bytes, ct);
    }

    public async Task<long> SalvarImagemOtimizadaAsync(IFormFile arquivo, int maxDimensao = OtimizadorDeImagem.MaxDimensaoPadrao, int qualidade = OtimizadorDeImagem.QualidadeJpegPadrao, bool comMarcaDagua = false, CancellationToken ct = default)
    {
        var bytes = await LerBytesAsync(arquivo, ct);
        var logotipo = comMarcaDagua ? await ObterLogotipoMarcaDaguaAsync(ct) : null;

        var otimizada = await OtimizadorDeImagem.OtimizarAsync(bytes, arquivo.ContentType, maxDimensao, qualidade, logotipo, ct)
            ?? throw new InvalidOperationException("Formato não suportado: envie uma imagem JPG, PNG ou WebP.");

        return await SalvarBytesAsync(
            OtimizadorDeImagem.NomeComExtensao(arquivo.FileName, otimizada.Extensao),
            otimizada.ContentType, otimizada.Bytes, ct);
    }

    public async Task<long> SalvarThumbnailAsync(IFormFile arquivo, int maxDimensao = 400, int qualidade = 75, CancellationToken ct = default)
    {
        var bytes = await LerBytesAsync(arquivo, ct);
        var otimizada = await OtimizadorDeImagem.OtimizarAsync(bytes, arquivo.ContentType, maxDimensao, qualidade, ct: ct)
            ?? throw new InvalidOperationException("Formato não suportado: envie uma imagem JPG, PNG ou WebP.");

        return await SalvarBytesAsync(
            OtimizadorDeImagem.NomeComExtensao(arquivo.FileName, otimizada.Extensao),
            otimizada.ContentType, otimizada.Bytes, ct);
    }

    /// <summary>Lê o upload inteiro para memória — os bytes vão para o banco.</summary>
    private static async Task<byte[]> LerBytesAsync(IFormFile arquivo, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await arquivo.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    /// <summary>
    /// Logotipo do portal (configuração "logo-principal") usado na marca d'água da
    /// galeria. Vem daqui, e não do otimizador, para a regra de imagem continuar
    /// sem depender de banco.
    /// </summary>
    private async Task<byte[]?> ObterLogotipoMarcaDaguaAsync(CancellationToken ct) =>
        await _db.ConfiguracoesSite.AsNoTracking()
            .Where(c => c.Chave == "logo-principal" && c.ArquivoId != null)
            .Select(c => c.Arquivo!.Bytes)
            .FirstOrDefaultAsync(ct);

    public async Task<long> SalvarFaviconAsync(IFormFile arquivo, int dimensao = 64, CancellationToken ct = default)
    {
        try
        {
            await using var origem = arquivo.OpenReadStream();
            using var imagem = await Image.LoadAsync(origem, ct);

            // Redimensiona só para reduzir (nunca amplia), encaixando em um
            // quadrado dimensao×dimensao — PNG quadrado de fonte vira 64×64.
            if (imagem.Width > dimensao || imagem.Height > dimensao)
            {
                imagem.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(dimensao, dimensao)
                }));
            }

            using var ms = new MemoryStream();
            await imagem.SaveAsync(ms, new PngEncoder { SkipMetadata = true }, ct);
            return await SalvarBytesAsync(arquivo.FileName, "image/png", ms.ToArray(), ct);
        }
        catch (OperationCanceledException) { throw; }
        catch
        {
            // Não-imagem (ex.: .ico) ou decode falhou: salva como está — o
            // favicon nunca deve travar por causa de um formato inesperado.
            return await SalvarAsync(arquivo, ct);
        }
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

    public async Task<byte[]?> GerarPngRedimensionadoAsync(long arquivoId, int maxDimensao, CancellationToken ct = default)
    {
        try
        {
            var arquivo = await _db.Arquivos.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == arquivoId, ct);
            if (arquivo?.Bytes is not { Length: > 0 })
                return null;

            using var imagem = await Image.LoadAsync(new MemoryStream(arquivo.Bytes), ct);
            imagem.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxDimensao, maxDimensao)
            }));

            using var ms = new MemoryStream();
            await imagem.SaveAsync(ms, new PngEncoder { SkipMetadata = true }, ct);
            return ms.ToArray();
        }
        catch (OperationCanceledException) { throw; }
        catch
        {
            // Não-imagem (ex.: .ico): impossível derivar um PNG — sinaliza com null.
            return null;
        }
    }

    public async Task<(byte[] Bytes, string ContentType, string Extensao)?> GerarRedimensionadoAsync(long arquivoId, int larguraMaxima, CancellationToken ct = default)
    {
        // Só os bytes importam: a projeção evita trazer o registro inteiro.
        var arquivo = await _db.Arquivos.AsNoTracking()
            .Where(a => a.Id == arquivoId)
            .Select(a => new { a.Bytes, a.ContentType })
            .FirstOrDefaultAsync(ct);

        if (arquivo?.Bytes is not { Length: > 0 })
            return null;

        // Mesma regra de imagem do upload (ver OtimizadorDeImagem): aqui não existe mais
        // resize/encode próprio. A versão antiga duplicava a regra e mantinha o EXIF no
        // arquivo derivado — ou seja, o GPS da foto viajava em cada miniatura servida com
        // cache de um ano, e os pixels saíam sem a rotação aplicada, deixando a orientação
        // na mão de quem for consumir a imagem.
        var reduzida = await OtimizadorDeImagem.ReduzirAsync(
            arquivo.Bytes, arquivo.ContentType, larguraMaxima, ct: ct);

        return reduzida is null
            ? null
            : (reduzida.Bytes, reduzida.ContentType, reduzida.Extensao);
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
