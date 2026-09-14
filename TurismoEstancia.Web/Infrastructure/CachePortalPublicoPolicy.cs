using Microsoft.AspNetCore.OutputCaching;

namespace TurismoEstancia.Web.Infrastructure;

/// <summary>
/// Regra do cache de página do portal: cacheia <b>apenas</b> GET/HEAD de página
/// pública com visitante anônimo. Todo o resto fica de fora — em especial
/// qualquer requisição autenticada (é o que protege o painel e o Identity, hoje
/// e em qualquer rota futura) e qualquer método que não seja leitura.
///
/// Toda entrada sai com a mesma tag (<see cref="TagConteudo"/>): uma escrita no
/// CMS descarta o portal inteiro de uma vez, então a edição aparece no próximo
/// acesso (ver <c>InvalidaCacheConteudoMiddleware</c>).
///
/// Como as páginas do portal emitem cookies do visitante (sessão anônima e
/// anti-forgery), a resposta guardada os contém; quem evita que eles cheguem a
/// outro visitante é o <c>CookiesDeVisitanteMiddleware</c>, que os troca na saída.
/// </summary>
public sealed class CachePortalPublicoPolicy : IOutputCachePolicy
{
    /// <summary>Tag única de invalidação: todo o conteúdo do portal.</summary>
    public const string TagConteudo = "conteudo-portal";

    /// <summary>
    /// Chave em <c>HttpContext.Items</c> marcada quando a resposta é servida do
    /// cache. Marcada aqui (o único ponto que só roda em acerto de cache) e lida
    /// pelo <c>CookiesDeVisitanteMiddleware</c>, que troca os cookies da entrada
    /// pelos deste visitante.
    /// </summary>
    public const string ItemRespostaDoCache = "turismo.resposta-do-cache";

    /// <summary>
    /// Teto de segurança. As edições do CMS invalidam na hora (tag), então este
    /// tempo só cobre o que não passa pelo painel — hoje, os contadores de
    /// curtida/visualização da galeria (o lightbox já atualiza o número por JS).
    /// </summary>
    public static readonly TimeSpan Duracao = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Parâmetros de query que mudam o conteúdo: paginação da galeria, das
    /// notícias e da agenda (que tem dois paginadores). Sem eles o cache serviria
    /// sempre a primeira página para qualquer ?pagina=N.
    /// </summary>
    private static readonly string[] ChavesQueVariam = { "pagina", "paginaPassados" };

    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        var http = context.HttpContext;

        // Só leitura (POST/PUT/DELETE nunca são cacheados).
        if (!HttpMethods.IsGet(http.Request.Method) && !HttpMethods.IsHead(http.Request.Method))
            return Descartar(context);

        // Só visitante anônimo: resposta de usuário logado nunca entra no cache.
        if (http.User.Identity?.IsAuthenticated == true)
            return Descartar(context);

        // Só página pública do portal (mesma definição do rastreio de visitas).
        if (!RotasPortal.EhPaginaPublica(http.Request.Path.Value ?? "/"))
            return Descartar(context);

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        // Evita renderizar a mesma página N vezes em paralelo: a primeira
        // requisição preenche o cache e as concorrentes esperam por ela.
        context.AllowLocking = true;
        context.ResponseExpirationTimeSpan = Duracao;
        context.Tags.Add(TagConteudo);
        context.CacheVaryByRules.QueryKeys = ChavesQueVariam;

        return ValueTask.CompletedTask;
    }

    /// <summary>Roda só quando a resposta veio do cache — é o sinal para trocar os cookies.</summary>
    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        context.HttpContext.Items[ItemRespostaDoCache] = true;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Depois de renderizar, só as páginas que deram certo entram no cache. Sem isso
    /// uma varredura de URLs inexistentes guardaria a página de erro de cada uma e
    /// encheria a memória à toa (basta inventar endereços aleatórios).
    /// </summary>
    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        if (context.HttpContext.Response.StatusCode != StatusCodes.Status200OK)
            context.AllowCacheStorage = false;

        return ValueTask.CompletedTask;
    }

    /// <summary>Fora das regras: nem lê do cache nem guarda no cache.</summary>
    private static ValueTask Descartar(OutputCacheContext context)
    {
        context.EnableOutputCaching = false;
        context.AllowCacheLookup = false;
        context.AllowCacheStorage = false;
        return ValueTask.CompletedTask;
    }
}
