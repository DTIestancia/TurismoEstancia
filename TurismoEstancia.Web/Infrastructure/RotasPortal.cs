namespace TurismoEstancia.Web.Infrastructure;

/// <summary>
/// Definição única de "página pública do portal": o que é página que o visitante
/// navega e o que é painel/infraestrutura. Fica num só lugar porque duas decisões
/// dependem disso e precisam sempre concordar — o rastreio de visitas (analytics)
/// e o cache de página (só página pública anônima é cacheada).
/// </summary>
public static class RotasPortal
{
    /// <summary>
    /// Prefixos que NÃO são páginas públicas: painel, login, mídias servidas por
    /// endpoint, estáticos, APIs e SEO. Inclui <c>/antiforgery</c>: o token é
    /// pessoal (jamais pode vir do cache) e não é uma visita.
    /// </summary>
    private static readonly string[] PrefixosNaoPublicos =
    {
        "/Gerenciador", "/Operador", "/arquivo/", "/api/", "/css/", "/js/", "/lib/",
        "/img", "/images", "/favicon", "/sitemap.xml", "/robots.txt",
        "/Account", "/Identity", "/Home/Error", "/Privacy", "/Evento/", "/antiforgery"
    };

    /// <summary>True para rotas do portal público (o que o visitante navega).</summary>
    public static bool EhPaginaPublica(string rota)
    {
        if (rota == "/") return true;
        foreach (var prefixo in PrefixosNaoPublicos)
        {
            if (rota.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase))
                return false;
        }
        return true;
    }

    /// <summary>
    /// True para rotas de escrita do CMS. Toda edição do painel (criar, editar,
    /// excluir, ocultar, reativar) é um POST dentro de uma dessas áreas.
    /// </summary>
    public static bool EhEscritaDoPainel(string rota) =>
        rota.StartsWith("/Gerenciador", StringComparison.OrdinalIgnoreCase) ||
        rota.StartsWith("/Operador", StringComparison.OrdinalIgnoreCase);
}
