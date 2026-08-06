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
        try
        {
            var arquivo = await _arquivos.ObterAsync(id, ct);

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
