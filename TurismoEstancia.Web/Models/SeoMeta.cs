namespace TurismoEstancia.Web.Models;

/// <summary>
/// Metadados SEO de uma página do portal: title, meta description, canonical,
/// Open Graph e Twitter Cards. As páginas preenchem os campos que conhecem
/// (título, descrição, imagem, tipo) e o SeoService complementa com as
/// configurações do site (título do site, meta descrição, logotipo).
/// </summary>
public class SeoMeta
{
    /// <summary>Título da página (ex.: "Praia do Saco"). Vazio = só o nome do site.</summary>
    public string? Titulo { get; set; }

    /// <summary>Descrição da página. Vazia = usa a meta descrição configurada.</summary>
    public string? Descricao { get; set; }

    /// <summary>Imagem de compartilhamento ("/arquivo/5" ou URL absoluta). Padrão: logotipo.</summary>
    public string? ImagemUrl { get; set; }

    /// <summary>Tipo Open Graph: "website" ou "article".</summary>
    public string Tipo { get; set; } = "website";

    /// <summary>Data de publicação em ISO 8601 (apenas em páginas "article").</summary>
    public string? DataPublicacao { get; set; }

    /// <summary>Página que não deve ser indexada (404/500, privacidade).</summary>
    public bool NoIndex { get; set; }

    // ---- Preenchidos pelo SeoService a partir das configurações do site ----

    /// <summary>Título do site (config "site-titulo").</summary>
    public string NomeSite { get; set; } = "Descubra Estância";

    /// <summary>Meta descrição padrão (config "meta-descricao").</summary>
    public string? SiteDescricao { get; set; }
}
