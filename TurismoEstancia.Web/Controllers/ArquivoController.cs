using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Web.Controllers;

/// <summary>
/// Serve os binários (imagens, vídeos, PDFs) gravados na tabela Arquivo.
///
/// <b>Nada passa por disco:</b> o binário é lido do banco em janelas de memória
/// (<c>FluxoDoArquivoNoBanco</c>, busca sequencial do <c>varbinary(max)</c>) e as
/// versões reduzidas de <c>?largura=N</c> são derivadas na memória e guardadas no
/// cache de memória do processo — nada é gravado no deploy, no perfil do serviço
/// ou em qualquer pasta da máquina.
/// </summary>
public class ArquivoController : Controller
{
    /// <summary>Prefixo das chaves das versões reduzidas no cache de memória.</summary>
    private const string PrefixoDaDerivacao = "arquivo-derivado-";

    /// <summary>
    /// Custo fixo por entrada no cache (metadados do objeto), somado ao dos bytes —
    /// é o que mantém o limite de memória do cache proporcional ao que ele guarda.
    /// </summary>
    private const long CustoFixoDaEntrada = 512;

    /// <summary>Quanto tempo uma versão reduzida fica em memória antes de ser derivada de novo.</summary>
    private static readonly TimeSpan DuracaoDaDerivacao = TimeSpan.FromMinutes(30);

    // Um semáforo por (arquivo, largura): mídias diferentes geram em paralelo (a
    // geração é CPU-bound e o .NET paraleliza entre núcleos); a mesma derivada nunca
    // é gerada duas vezes ao mesmo tempo.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaforos = new();

    private readonly IArquivoService _arquivos;
    private readonly IMemoryCache _cache;

    public ArquivoController(IArquivoService arquivos, IMemoryCache cache)
    {
        _arquivos = arquivos;
        _cache = cache;
    }

    /// <summary>
    /// GET /arquivo/{id} — devolve o binário com o Content-Type correto.
    /// Com <c>?largura=N</c> (200–2560), devolve a foto reduzida com a mesma regra de
    /// imagem do upload; a versão fica em cache de memória (os arquivos são
    /// imutáveis, então a chave nunca fica velha durante a vida do processo).
    /// </summary>
    [HttpGet]
    [Route("arquivo/{id:long}")]
    public async Task<IActionResult> Index(long id, [FromQuery] int? largura, CancellationToken ct)
    {
        // Proteção contra hotlink: bloqueia quem carrega a imagem a partir de
        // OUTRO site (Referer de host diferente). Acesso direto (sem Referer —
        // nova aba, OG/redes sociais) continua permitido, e o portal/painel
        // sempre enviam o próprio host. Robôs de busca e redes sociais passam
        // pelo User-Agent — sem isso, Google Imagens e a inspeção do Search
        // Console recebem 403 e as fotos somem dos resultados.
        var userAgent = Request.Headers.UserAgent.ToString();
        if (!EhRoboConhecido(userAgent))
        {
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
        }

        if (largura is >= 200 and <= 2560)
        {
            var derivada = await DerivarAsync(id, largura.Value, ct);

            // Sem bytes derivados (foto já menor que o pedido, mídia que não se
            // redimensiona — vídeo, GIF animado, SVG — ou id inexistente): o
            // original é a resposta certa e o fluxo segue abaixo.
            if (derivada.Bytes is { Length: > 0 } reduzida)
            {
                // Miniaturas da geração atual: o ETag dispensa revalidar os pixels.
                Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                var etagReduzida = $"\"{id}-{largura}\"";
                if (Request.Headers.IfNoneMatch.ToString() == etagReduzida)
                    return StatusCode(StatusCodes.Status304NotModified);

                Response.Headers.ETag = etagReduzida;
                return File(reduzida, derivada.ContentType);
            }
        }

        var aberto = await _arquivos.AbrirAsync(id, ct);
        if (aberto is null)
            return NotFound();

        var (fluxo, metadados) = aberto.Value;

        // Arquivos da tabela são IMUTÁVEIS para o upload (sempre cria um novo
        // registro; substituir = excluir o antigo + gravar outro), então o cache
        // pode ser longo: imagens 1 ano + immutable (o navegador nem revalida),
        // demais mídias 7 dias. O único caso que troca os bytes no lugar é o
        // comando "recomprimir-imagens" (manutenção, com o portal parado): por
        // isso o ETag leva também o tamanho — quem tem a versão antiga revalida
        // e recebe a nova. O 403 do hotlink não recebe Cache-Control, então
        // nunca é cacheado.
        var eImagem = metadados.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        Response.Headers.CacheControl = eImagem
            ? "public, max-age=31536000, immutable"
            : "public, max-age=604800";

        var etag = $"\"{id}-{metadados.CriadoEm.Ticks}-{metadados.Size}\"";
        if (Request.Headers.IfNoneMatch.ToString() == etag)
        {
            await fluxo.DisposeAsync();
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers.ETag = etag;

        // O framework copia do fluxo para a resposta em blocos, buscando do banco
        // conforme avança, e atende Range porque o fluxo é pesquisável — é o que
        // permite arrastar a barra do player sem nenhuma cópia local do vídeo.
        return File(fluxo, metadados.ContentType, enableRangeProcessing: true);
    }

    /// <summary>
    /// Versão reduzida de <c>?largura=N</c>, derivada na primeira visita e mantida
    /// no cache de memória. O veredito "não há derivada" também é guardado: sem
    /// isso, cada requisição de um vídeo ou de uma foto já pequena releria o blob
    /// inteiro só para descobrir de novo que ele não muda.
    /// </summary>
    private async Task<Derivada> DerivarAsync(long id, int largura, CancellationToken ct)
    {
        var chave = $"{PrefixoDaDerivacao}{id}-{largura}";

        if (_cache.TryGetValue(chave, out Derivada guardada))
            return guardada;

        var traca = _semaforos.GetOrAdd(chave, _ => new SemaphoreSlim(1, 1));
        await traca.WaitAsync(ct);
        try
        {
            // Duas requisições do mesmo tamanho ao mesmo tempo: a segunda espera e
            // aproveita o que a primeira guardou.
            if (_cache.TryGetValue(chave, out guardada))
                return guardada;

            var mini = await _arquivos.GerarRedimensionadoAsync(id, largura, ct);
            var derivada = mini is null
                ? new Derivada(null, string.Empty)
                : new Derivada(mini.Value.Bytes, mini.Value.ContentType);

            _cache.Set(chave, derivada, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = DuracaoDaDerivacao,
                Size = (derivada.Bytes?.LongLength ?? 0) + CustoFixoDaEntrada
            });

            return derivada;
        }
        finally
        {
            traca.Release();
            _semaforos.TryRemove(chave, out _);
        }
    }

    /// <summary>Robôs de busca e pré-visualizadores (User-Agent): passam pelo hotlink
    /// para imagens e vídeos serem indexados e inspecionados normalmente.</summary>
    private static bool EhRoboConhecido(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return false;
        return userAgent.Contains("bot", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("crawl", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("spider", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("mediapartners-google", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("facebookexternalhit", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("twitterbot", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("linkedinbot", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("whatsapp", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("telegrambot", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("slackbot", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("discordbot", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Versão reduzida em cache. <see cref="Bytes"/> nulo significa "não existe
    /// derivada para este pedido: sirva o original" — o veredito é guardado tanto
    /// quanto a imagem.
    /// </summary>
    private readonly record struct Derivada(byte[]? Bytes, string ContentType);
}
