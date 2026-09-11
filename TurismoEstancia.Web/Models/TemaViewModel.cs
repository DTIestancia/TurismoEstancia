using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Web.Models;

/// <summary>
/// Tema do portal: as 6 cores da paleta oficial + a cor de fundo de cada
/// seção da home, editáveis pelo Gerenciador. Cada cor vira uma configuração
/// (chaves <c>tema-cor-*</c> e <c>tema-secao-*</c>, tipo Texto) e o
/// <see cref="TurismoEstancia.Web.Components.ThemeSiteViewComponent"/> emite o
/// CSS em tempo de execução — sem recompilar o SCSS. As ondulações entre
/// seções usam a cor da seção seguinte, então acompanham automaticamente.
/// </summary>
public class TemaViewModel
{
    public const string ChaveVermelho = "tema-cor-vermelho";
    public const string ChaveLaranja = "tema-cor-laranja";
    public const string ChaveAmarelo = "tema-cor-amarelo";
    public const string ChaveVerde = "tema-cor-verde";
    public const string ChaveAzul = "tema-cor-azul";
    public const string ChaveRosa = "tema-cor-rosa";

    public const string ChaveSecaoHistoria = "tema-secao-historia";
    public const string ChaveSecaoConheca = "tema-secao-conheca";
    public const string ChaveSecaoMaravilhas = "tema-secao-maravilhas";
    public const string ChaveSecaoAgenda = "tema-secao-agenda";
    public const string ChaveSecaoRoteiros = "tema-secao-roteiros";
    public const string ChaveSecaoNoticias = "tema-secao-noticias";
    public const string ChaveSecaoMapa = "tema-secao-mapa";
    public const string ChaveSecaoRodape = "tema-secao-rodape";

    [Display(Name = "Vermelho")]
    public string? Vermelho { get; set; } = "#ED2027";

    [Display(Name = "Laranja")]
    public string? Laranja { get; set; } = "#F97E31";

    [Display(Name = "Amarelo")]
    public string? Amarelo { get; set; } = "#FCBB0F";

    [Display(Name = "Verde")]
    public string? Verde { get; set; } = "#658746";

    [Display(Name = "Azul")]
    public string? Azul { get; set; } = "#0095F6";

    [Display(Name = "Rosa")]
    public string? Rosa { get; set; } = "#E9568A";

    [Display(Name = "Nossa Cidade")]
    public string? SecaoHistoria { get; set; } = "#C76527";

    [Display(Name = "Conheça Estância")]
    public string? SecaoConheca { get; set; } = "#060D1A";

    [Display(Name = "7 Maravilhas")]
    public string? SecaoMaravilhas { get; set; } = "#030E19";

    [Display(Name = "Agenda")]
    public string? SecaoAgenda { get; set; } = "#320100";

    [Display(Name = "Planeje sua viagem")]
    public string? SecaoRoteiros { get; set; } = "#001326";

    [Display(Name = "Notícias e Blog")]
    public string? SecaoNoticias { get; set; } = "#FFFFFF";

    [Display(Name = "Mapa")]
    public string? SecaoMapa { get; set; } = "#030E19";

    [Display(Name = "Rodapé")]
    public string? SecaoRodape { get; set; } = "#001326";

    /// <summary>True quando ao menos uma cor foi personalizada no banco.</summary>
    public bool Personalizado { get; set; }
}
