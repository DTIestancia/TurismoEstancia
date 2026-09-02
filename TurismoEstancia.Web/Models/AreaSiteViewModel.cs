using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Web.Models;

/// <summary>
/// Edição de uma área do portal (Hero, Nossa Cidade, Cultura...) num formulário
/// só: textos por chave + imagens (upload) + atalhos para os CRUDs relacionados
/// (grupos, pratos, eventos...) + dados extras específicos da área (slides do
/// hero, estatísticas da cidade).
/// </summary>
public class AreaSiteViewModel
{
    /// <summary>Slug da área (hero, cidade, cultura...).</summary>
    public string Area { get; set; } = "";

    public string Titulo { get; set; } = "";
    public string Descricao { get; set; } = "";

    /// <summary>URL da seção correspondente no portal (ex.: "/#section-cultura") — para o botão "Ver seção no portal".</summary>
    public string? VerSecaoUrl { get; set; }

    public List<CampoTextoArea> Textos { get; set; } = new();
    public List<CampoImagemArea> Imagens { get; set; } = new();

    /// <summary>Atalhos para telas de lista relacionadas (CRUDs existentes).</summary>
    public List<LinkArea> Links { get; set; } = new();

    /// <summary>Itens já cadastrados na área (cards na tela) + rota de cadastro.</summary>
    public ItensAreaViewModel? Itens { get; set; }

    // ---- Extras por área ----
    public IReadOnlyList<SlideDto> Slides { get; set; } = Array.Empty<SlideDto>();
    public IReadOnlyList<EstatisticaDto> Estatisticas { get; set; } = Array.Empty<EstatisticaDto>();

    /// <summary>Vídeo institucional (fundo do hero) — gerenciado na área Hero.</summary>
    public long? VideoArquivoId { get; set; }

    public bool ExibeSlides => Slides.Count > 0;
    public bool ExibeEstatisticas => Estatisticas.Count > 0;
}

/// <summary>Campo de texto de uma área (uma chave de conteúdo do portal).</summary>
public class CampoTextoArea
{
    public required string Chave { get; init; }
    public required string Nome { get; init; }
    public string? Dica { get; init; }

    /// <summary>True quando o texto aceita HTML (<strong>, <br>) — a view mostra o preview.</summary>
    public bool AceitaHtml { get; init; } = true;

    public string? Valor { get; set; }
}

/// <summary>Campo de imagem de uma área (chave que guarda o id de arquivo).</summary>
public class CampoImagemArea
{
    public required string Chave { get; init; }
    public required string Nome { get; init; }

    /// <summary>Id do arquivo atual (para mostrar o preview) — null quando não há imagem.</summary>
    public long? ArquivoId { get; set; }

    public string? Valor { get; set; }
}

/// <summary>Atalho para uma tela de lista do Gerenciador (CRUD existente).</summary>
public class LinkArea
{
    public required string Titulo { get; init; }
    public required string Descricao { get; init; }
    public required string Url { get; init; }
    public string Icone { get; init; } = "arrow-right";
}

/// <summary>Itens já cadastrados de uma área (exibidos em cards na tela da área).</summary>
public class ItensAreaViewModel
{
    /// <summary>Rótulo da seção de itens (ex.: "Pratos turísticos cadastrados").</summary>
    public required string Titulo { get; init; }

    /// <summary>Texto do botão de cadastro (ex.: "Cadastrar novo prato").</summary>
    public required string RotuloBotao { get; init; }

    /// <summary>Rota de cadastro (abre no modal).</summary>
    public required string UrlCriar { get; init; }

    /// <summary>Rota da lista completa (link "ver todos").</summary>
    public required string UrlLista { get; init; }

    /// <summary>Ícone do bloco (lucide).</summary>
    public string Icone { get; init; } = "box";

    public IReadOnlyList<ItemAreaViewModel> Itens { get; init; } = Array.Empty<ItemAreaViewModel>();
}

/// <summary>Um item já cadastrado (card na tela da área).</summary>
public class ItemAreaViewModel
{
    public required string Nome { get; init; }
    public string? Detalhe { get; init; }
    public long? ImagemArquivoId { get; init; }
    public bool Ativo { get; init; } = true;
    public int Id { get; init; }
}
