namespace TurismoEstancia.Web.Models;

/// <summary>Card de contagem exibido nos dashboards do painel.</summary>
public class PainelStatViewModel
{
    public string Rotulo { get; set; } = string.Empty;
    public string Icone { get; set; } = "info";
    public int Valor { get; set; }
}
