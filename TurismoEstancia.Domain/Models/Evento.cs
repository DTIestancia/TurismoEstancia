namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Evento da agenda. O portal exibe apenas eventos futuros
/// (<c>DataFim &gt;= hoje</c>) e o botão "Adicionar à Agenda" gera um arquivo <c>.ics</c>.
/// </summary>
public class Evento
{
    public int Id { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descricao { get; set; }

    public string? Local { get; set; }

    public DateTime DataInicio { get; set; }

    public DateTime DataFim { get; set; }

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;
}
