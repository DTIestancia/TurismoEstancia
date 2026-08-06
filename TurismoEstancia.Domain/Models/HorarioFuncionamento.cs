namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Horário de funcionamento de um ponto por dia da semana.
/// Se <see cref="Fechado"/> for true, os horários devem ficar vazios.
/// </summary>
public class HorarioFuncionamento
{
    public int Id { get; set; }

    public int PontoTuristicoId { get; set; }

    public PontoTuristico? PontoTuristico { get; set; }

    public DiaSemana DiaSemana { get; set; }

    public TimeOnly? HoraInicio { get; set; }

    public TimeOnly? HoraFim { get; set; }

    /// <summary>Indica que o ponto não abre nesse dia.</summary>
    public bool Fechado { get; set; }
}
