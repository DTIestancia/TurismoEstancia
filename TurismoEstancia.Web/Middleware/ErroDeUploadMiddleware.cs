using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TurismoEstancia.Services.Infra.Arquivos;

namespace TurismoEstancia.Web.Middleware;

/// <summary>
/// Quando o formulário passa do <b>teto de transporte</b> da requisição (60 MB, ver
/// <c>InfrastructureExtensions</c>), o Kestrel/IIS corta a leitura do corpo e o ASP.NET
/// lança um 413 — o operador veria uma página de erro crua, sem saber o que fazer.
///
/// Este middleware intercepta exatamente esse caso e devolve o operador para a tela
/// onde ele estava, com a explicação no alerta do painel (<c>TempData["PainelErro"]</c>,
/// o mesmo canal dos demais erros de upload). Um vídeo gigante não chega a acionar o
/// limite por arquivo (10 MB) justamente porque o servidor recusa antes — é aqui que
/// esse caminho fecha.
/// </summary>
public sealed class ErroDeUploadMiddleware
{
    /// <summary>Mensagem única do caso, mostrada no painel.</summary>
    public const string Mensagem =
        "O arquivo enviado é grande demais para o servidor receber de uma vez. " +
        "O limite é 10 MB por vídeo e 5 MB por imagem (e 60 MB no total de um envio com várias fotos). " +
        "Reduza o arquivo — ou envie as fotos em grupos menores — e tente de novo.";

    private readonly RequestDelegate _next;
    private readonly ILogger<ErroDeUploadMiddleware> _log;

    public ErroDeUploadMiddleware(RequestDelegate next, ILogger<ErroDeUploadMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext context, ITempDataDictionaryFactory tempData)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (CorpoGrandeDemais(ex) && !context.Response.HasStarted)
        {
            _log.LogWarning(
                ex,
                "Upload recusado pelo teto de transporte em {Metodo} {Caminho}.",
                context.Request.Method,
                context.Request.Path);

            var dicionario = tempData.GetTempData(context);
            dicionario["PainelErro"] = Mensagem;
            dicionario.Save();

            // Volta para onde o formulário estava (mesmo host); sem Referer confiável,
            // o painel é o destino seguro.
            context.Response.Redirect(VoltarPara(context), permanent: false, preserveMethod: false);
        }
    }

    /// <summary>O 413 do servidor ou o estouro do limite de corpo do multipart.</summary>
    private static bool CorpoGrandeDemais(Exception ex) =>
        (ex is BadHttpRequestException grande && grande.StatusCode == StatusCodes.Status413PayloadTooLarge)
        || (ex is InvalidDataException multipart
            && multipart.Message.Contains("Multipart body length limit", StringComparison.OrdinalIgnoreCase));

    /// <summary>O Referer, quando aponta para o próprio portal; senão, o painel.</summary>
    private static string VoltarPara(HttpContext context)
    {
        const string padrao = "/Gerenciador";

        var referer = context.Request.Headers.Referer.ToString();
        if (string.IsNullOrWhiteSpace(referer)
            || !Uri.TryCreate(referer, UriKind.Absolute, out var uri)
            || !string.Equals(uri.Host, context.Request.Host.Host, StringComparison.OrdinalIgnoreCase))
        {
            return padrao;
        }

        var caminho = uri.PathAndQuery;
        return caminho.StartsWith('/') && !caminho.StartsWith("//", StringComparison.Ordinal)
            ? caminho
            : padrao;
    }
}
