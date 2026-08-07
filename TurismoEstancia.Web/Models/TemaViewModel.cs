using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Web.Models;

/// <summary>
/// Tema do portal: as 6 cores da paleta oficial, editáveis pelo Gerenciador.
/// Cada cor vira uma configuração (chave <c>tema-cor-*</c>, tipo Texto) e o
/// <see cref="TurismoEstancia.Web.Components.ThemeSiteViewComponent"/> emite o
/// CSS em tempo de execução — sem recompilar o SCSS.
/// </summary>
public class TemaViewModel
{
    public const string ChaveVermelho = "tema-cor-vermelho";
    public const string ChaveLaranja = "tema-cor-laranja";
    public const string ChaveAmarelo = "tema-cor-amarelo";
    public const string ChaveVerde = "tema-cor-verde";
    public const string ChaveAzul = "tema-cor-azul";
    public const string ChaveRosa = "tema-cor-rosa";

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

    /// <summary>True quando ao menos uma cor foi personalizada no banco.</summary>
    public bool Personalizado { get; set; }
}
