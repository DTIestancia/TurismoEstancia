using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Web.Controllers;

/// <summary>Serve arquivos binários (imagens, vídeos, PDFs) gravados na tabela Arquivo.</summary>
public class ArquivoController : Controller
{
    private readonly IArquivoService _arquivos;

    public ArquivoController(IArquivoService arquivos) => _arquivos = arquivos;

    /// <summary>GET /arquivo/{id} — devolve o binário com o Content-Type correto.</summary>
    [Route("arquivo/{id:long}")]
    public async Task<IActionResult> Index(long id, CancellationToken ct)
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
}
