using Microsoft.Net.Http.Headers;
using TurismoEstancia.Web.Infrastructure;

namespace TurismoEstancia.Web.Middleware;

/// <summary>
/// Impede que a página servida do cache entregue a identidade de outro visitante.
///
/// Contexto: o cache de página guarda a resposta inteira — corpo <b>e cabeçalhos</b> —
/// e as páginas do portal emitem cookies próprios do visitante (a sessão anônima do
/// analytics e o cookie anti-forgery dos formulários públicos). O primeiro visitante
/// que renderiza a página deixa os cookies dele dentro da entrada, e sem este
/// middleware todos os seguintes recebem os mesmos: o contador de visitantes únicos
/// colapsa em um, e o segredo do anti-forgery deixa de ser segredo (quem quiser lê o
/// token e o cookie no HTML público e forja o POST em nome de outro visitante).
///
/// Por que aqui e não dentro do cache: nesta página o Razor descarrega a resposta
/// durante a renderização (o corpo passa de 16 KB), então quando o cache já
/// terminou de montar a entrada os cabeçalhos estão travados — não dá para tirar
/// os cookies do que foi guardado. A janela em que os cabeçalhos ainda aceitam
/// mudança é o <c>OnStarting</c>, imediatamente antes de a resposta começar a sair:
/// é ali que os cookies herdados do cache são descartados e o cookie da sessão
/// deste visitante é (re)colocado. Como os cookies do visitante atual e os do
/// cache são montados da mesma forma, a troca é invisível para quem navega.
///
/// A limpeza só acontece na resposta que veio do cache (a política marca o
/// request). Numa resposta recém-renderizada os cookies são os do próprio
/// visitante e vão embora como sempre.
/// </summary>
public class CookiesDeVisitanteMiddleware
{
    private readonly RequestDelegate _next;

    public CookiesDeVisitanteMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Só GET de página do portal (o resto é painel, mídia, estático). O tipo
        // do conteúdo só existe depois do endpoint, então é checado no OnStarting.
        if (HttpMethods.IsGet(context.Request.Method)
            && RotasPortal.EhPaginaPublica(context.Request.Path.Value ?? "/"))
        {
            context.Response.OnStarting(() =>
            {
                if (context.Items.ContainsKey(CachePortalPublicoPolicy.ItemRespostaDoCache)
                    && EhPaginaHtml(context))
                {
                    // Cookies que vieram junto com a página guardada.
                    RemoverCookies(context, valor =>
                        NomeDoCookie(valor) == SessaoAnonima.NomeCookie || EhAntiForgery(valor));
                }

                // A sessão anônima é sempre a deste visitante.
                if (context.Items.TryGetValue(SessaoAnonima.ItemSessaoId, out var sessao)
                    && sessao is string sessaoId
                    && !TemCookie(context, SessaoAnonima.NomeCookie))
                {
                    context.Response.Cookies.Append(
                        SessaoAnonima.NomeCookie, sessaoId, SessaoAnonima.OpcoesCookie());
                }

                return Task.CompletedTask;
            });
        }

        await _next(context);
    }

    /// <summary>True quando a resposta é uma página HTML do portal.</summary>
    private static bool EhPaginaHtml(HttpContext context)
    {
        var tipo = context.Response.ContentType;
        return tipo is not null && tipo.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Nome do anti-forgery: leva um sufixo por aplicação no próprio nome.</summary>
    private const string PrefixoAntiForgery = ".AspNetCore.Antiforgery";

    private static bool EhAntiForgery(string? valor)
    {
        var nome = NomeDoCookie(valor);
        return nome == PrefixoAntiForgery
            || nome.StartsWith(PrefixoAntiForgery + ".", StringComparison.Ordinal);
    }

    /// <summary>Nome do cookie: tudo antes do primeiro '=' (o atributo vem depois de ';').</summary>
    private static string NomeDoCookie(string? valor)
    {
        if (string.IsNullOrEmpty(valor)) return string.Empty;
        var fim = valor.IndexOf('=');
        return fim < 0 ? valor : valor[..fim];
    }

    /// <summary>True se a resposta leva algum cookie com este nome.</summary>
    private static bool TemCookie(HttpContext context, string nome)
    {
        foreach (var valor in context.Response.Headers.SetCookie)
        {
            if (NomeDoCookie(valor) == nome) return true;
        }
        return false;
    }

    /// <summary>Descarta os cookies que o filtro selecionar, mantendo os demais.</summary>
    private static void RemoverCookies(HttpContext context, Func<string?, bool> descartar)
    {
        var mantidos = context.Response.Headers.SetCookie
            .Where(valor => !descartar(valor))
            .ToArray();

        context.Response.Headers.Remove(HeaderNames.SetCookie);
        foreach (var valor in mantidos)
            context.Response.Headers.Append(HeaderNames.SetCookie, valor);
    }
}
