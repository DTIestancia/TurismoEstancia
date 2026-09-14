using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Web.Controllers;

/// <summary>Serve arquivos binários (imagens, vídeos, PDFs) gravados na tabela Arquivo.</summary>
public class ArquivoController : Controller
{
    private readonly IArquivoService _arquivos;
    private readonly IWebHostEnvironment _env;
    private static readonly SemaphoreSlim _semaforoMiniaturas = new(1, 1);

    public ArquivoController(IArquivoService arquivos, IWebHostEnvironment env)
    {
        _arquivos = arquivos;
        _env = env;
    }

    /// <summary>
    /// GET /arquivo/{id} — devolve o binário com o Content-Type correto.
    /// Com <c>?largura=N</c> (200–2560), devolve foto reduzida (cache em disco,
    /// gerada uma vez — os arquivos são imutáveis, então o cache nunca expira).
    /// </summary>
    [HttpGet]
    [Route("arquivo/{id:long}")]
    public async Task<IActionResult> Index(long id, [FromQuery] int? largura, CancellationToken ct)
    {
        // Proteção contra hotlink: bloqueia quem carrega a imagem a partir de
        // OUTRO site (Referer de host diferente). Acesso direto (sem Referer —
        // nova aba, OG/redes sociais, bot de busca) continua permitido, e o
        // portal/painel sempre enviam o próprio host.
        var referer = Request.Headers.Referer.ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            var host = Request.Host.Host;
            if (!Uri.TryCreate(referer, UriKind.Absolute, out var uri)
                || !string.Equals(uri.Host, host, StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }
        }

        try
        {
            // Miniatura: {ContentRoot}/cache/arquivo/{id}-{largura}.jpg|.png —
            // fora do wwwroot de propósito (StaticFiles não serve, sem bypass).
            if (largura is >= 200 and <= 2560)
            {
                var pasta = Path.Combine(_env.ContentRootPath, "cache", "arquivo");
                var baseNome = Path.Combine(pasta, $"{id}-{largura}");
                var doCache = LocalizarMiniatura(baseNome);

                if (doCache is null)
                {
                    await _semaforoMiniaturas.WaitAsync(ct);
                    try
                    {
                        doCache = LocalizarMiniatura(baseNome);
                        if (doCache is null)
                        {
                            var mini = await _arquivos.GerarRedimensionadoAsync(id, largura.Value, ct);
                            if (mini is not null)
                            {
                                Directory.CreateDirectory(pasta);
                                await System.IO.File.WriteAllBytesAsync(baseNome + mini.Value.Extensao, mini.Value.Bytes, ct);
                                doCache = baseNome + mini.Value.Extensao;
                            }
                        }
                    }
                    finally
                    {
                        _semaforoMiniaturas.Release();
                    }
                }

                if (doCache is not null)
                {
                    var tipoCache = doCache.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";
                    Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                    var etagCache = $"\"{id}-{largura}\"";
                    if (Request.Headers.IfNoneMatch.ToString() == etagCache)
                        return StatusCode(StatusCodes.Status304NotModified);

                    Response.Headers.ETag = etagCache;
                    return PhysicalFile(doCache, tipoCache, enableRangeProcessing: true);
                }
                // Sem miniatura (não-imagem ou já pequena): cai no original.
            }

            var arquivo = await _arquivos.ObterAsync(id, ct);

            // Arquivos da tabela são IMUTÁVEIS (upload sempre cria um novo registro;
            // substituir = excluir o antigo + gravar outro), então o cache pode ser
            // longo: imagens 1 ano + immutable (o navegador nem revalida), demais
            // mídias 7 dias. O ETag cobre revalidação (304) em navegadores/proxies
            // que ignoram o immutable. O 403 do hotlink não recebe Cache-Control,
            // então nunca é cacheado.
            var eImagem = arquivo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            Response.Headers.CacheControl = eImagem
                ? "public, max-age=31536000, immutable"
                : "public, max-age=604800";

            var etag = $"\"{id}-{arquivo.CriadoEm.Ticks}\"";
            if (Request.Headers.IfNoneMatch.ToString() == etag)
                return StatusCode(StatusCodes.Status304NotModified);

            Response.Headers.ETag = etag;
            return File(arquivo.Bytes, arquivo.ContentType, enableRangeProcessing: true);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>Localiza a miniatura em disco (.jpg ou .png) sem varrer o diretório.</summary>
    private static string? LocalizarMiniatura(string baseNome)
    {
        if (System.IO.File.Exists(baseNome + ".jpg"))
            return baseNome + ".jpg";
        if (System.IO.File.Exists(baseNome + ".png"))
            return baseNome + ".png";
        return null;
    }
}
