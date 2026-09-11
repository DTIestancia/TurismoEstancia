namespace TurismoEstancia.Web.Models;

/// <summary>Link de navegação do portal (fonte única dos 4 menus).</summary>
public sealed class LinkMenuPortal
{
    public string Rotulo { get; init; } = string.Empty;

    /// <summary>Âncora da home (#section-*) — usada na navbar; null = link de página.</summary>
    public string? Ancora { get; init; }

    /// <summary>URL da página (~/...) — usada fora da home.</summary>
    public string Url { get; init; } = "~/";

    /// <summary>Chave do data-section (scrollspy da navbar); null = sem destaque.</summary>
    public string? Secao { get; init; }
}

/// <summary>
/// Fonte única dos menus do portal (cidadão): navbar da home, header interno
/// e os dois rodapés. Rótulos seguem os títulos das páginas; a navbar usa as
/// âncoras das seções da home (+ Galeria, que é página).
/// </summary>
public static class MenuPortal
{
    /// <summary>Navbar da home: âncoras das seções + Galeria.</summary>
    public static IReadOnlyList<LinkMenuPortal> Navbar { get; } =
    [
        new() { Rotulo = "Home", Url = "~/", Secao = "home" },
        new() { Rotulo = "Nossa Cidade", Ancora = "#section-historia", Url = "~/cidade", Secao = "cidade" },
        new() { Rotulo = "Conheça Estância", Ancora = "#section-conheca", Url = "~/cultura", Secao = "conheca" },
        new() { Rotulo = "7 Maravilhas", Ancora = "#section-maravilhas", Url = "~/lugares", Secao = "maravilhas" },
        new() { Rotulo = "Agenda", Ancora = "#section-servicos", Url = "~/agenda", Secao = "agenda" },
        new() { Rotulo = "Planeje sua viagem", Ancora = "#section-roteiros", Url = "~/roteiros", Secao = "roteiros" },
        new() { Rotulo = "Blog", Ancora = "#section-noticias", Url = "~/noticias", Secao = "noticias" },
        new() { Rotulo = "Mapa", Ancora = "#section-mapa", Url = "~/", Secao = "mapa" },
    ];

    /// <summary>Header das páginas internas: só páginas.</summary>
    public static IReadOnlyList<LinkMenuPortal> Header { get; } =
    [
        new() { Rotulo = "Home", Url = "~/" },
        new() { Rotulo = "Nossa Cidade", Url = "~/cidade" },
        new() { Rotulo = "Nossa Cultura", Url = "~/cultura" },
        new() { Rotulo = "7 Maravilhas", Url = "~/lugares" },
        new() { Rotulo = "Agenda", Url = "~/agenda" },
        new() { Rotulo = "Planeje sua viagem", Url = "~/roteiros" },
        new() { Rotulo = "Blog", Url = "~/noticias" },
        new() { Rotulo = "Mídia kit", Url = "~/midia-kit" },
    ];

    /// <summary>Rodapés (home e páginas internas): catálogo completo de páginas.</summary>
    public static IReadOnlyList<LinkMenuPortal> Rodape { get; } =
    [
        new() { Rotulo = "Nossa Cidade", Url = "~/cidade" },
        new() { Rotulo = "Nossa Cultura", Url = "~/cultura" },
        new() { Rotulo = "Grupos Populares", Url = "~/grupos-populares" },
        new() { Rotulo = "Gastronomia", Url = "~/gastronomia" },
        new() { Rotulo = "Lugares que Encantam", Url = "~/lugares" },
        new() { Rotulo = "Galeria", Url = "~/galeria" },
        new() { Rotulo = "Agenda", Url = "~/agenda" },
        new() { Rotulo = "Planeje sua viagem", Url = "~/roteiros" },
        new() { Rotulo = "Blog", Url = "~/noticias" },
        new() { Rotulo = "Mídia kit", Url = "~/midia-kit" },
    ];
}
