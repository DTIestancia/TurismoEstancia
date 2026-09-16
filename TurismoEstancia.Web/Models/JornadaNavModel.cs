namespace TurismoEstancia.Web.Models;

/// <summary>Navegação "jornada" no fim das páginas de seção (voltar + continuar).</summary>
public class JornadaNavModel
{
    public required string AnteriorRotulo { get; init; }
    public required string AnteriorUrl { get; init; }
    public required string ProximoRotulo { get; init; }
    public required string ProximoUrl { get; init; }
}
