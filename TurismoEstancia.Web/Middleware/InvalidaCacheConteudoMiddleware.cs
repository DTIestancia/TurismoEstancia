using Microsoft.AspNetCore.OutputCaching;
using TurismoEstancia.Web.Infrastructure;

namespace TurismoEstancia.Web.Middleware;

/// <summary>
/// Descarta o cache das páginas do portal assim que o CMS grava algo.
///
/// Toda edição do painel é um POST em <c>/Gerenciador</c> ou <c>/Operador</c>, então
/// basta olhar o resultado da requisição: termina sem erro → o conteúdo do portal
/// mudou → as entradas cacheadas (todas com a mesma tag) saem na hora e a edição
/// aparece no próximo acesso. Ficou aqui, num ponto único, em vez de espalhar uma
/// chamada em cada ação de salvar do painel — assim nenhuma edição futura escapa.
/// </summary>
public class InvalidaCacheConteudoMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InvalidaCacheConteudoMiddleware> _logger;

    public InvalidaCacheConteudoMiddleware(RequestDelegate next, ILogger<InvalidaCacheConteudoMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IOutputCacheStore cache)
    {
        await _next(context);

        if (!HttpMethods.IsPost(context.Request.Method)) return;
        if (context.Response.StatusCode >= 400) return; // erro/validação: nada foi gravado
        if (!RotasPortal.EhEscritaDoPainel(context.Request.Path.Value ?? "")) return;

        // Só quem está logado pode ter gravado. O POST anônimo no painel (ou em
        // rota inexistente de lá) termina em 302 para o login — sem esta checagem
        // qualquer um descartaria o cache inteiro do portal em loop e o site
        // voltaria a bater no banco a cada visita.
        if (context.User.Identity?.IsAuthenticated != true) return;

        try
        {
            // Sem o token do request: a resposta já terminou e a invalidação não
            // pode ser cancelada por um visitante que fechou a aba.
            await cache.EvictByTagAsync(CachePortalPublicoPolicy.TagConteudo, CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Nunca derruba a requisição do painel por causa do cache — no pior
            // caso a página do portal fica velha até o teto de tempo da política.
            _logger.LogWarning(ex, "Falha ao descartar o cache do portal após escrita no CMS.");
        }
    }
}
