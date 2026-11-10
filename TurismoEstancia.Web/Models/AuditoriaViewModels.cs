using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Web.Models;

public sealed class AuditoriaListaViewModel
{
    public IReadOnlyList<AuditoriaAcao> Registros { get; init; } = [];
    public string? Tipo { get; init; }
    public string? Area { get; init; }
    public string? Usuario { get; init; }
    public DateTime? Inicio { get; init; }
    public DateTime? Fim { get; init; }
    public int Pagina { get; init; }
    public int TotalPaginas { get; init; }
    public int TotalRegistros { get; init; }
    public int TotalAcessos { get; init; }
    public int TotalAlteracoes { get; init; }
    public int TotalFalhas { get; init; }
}
