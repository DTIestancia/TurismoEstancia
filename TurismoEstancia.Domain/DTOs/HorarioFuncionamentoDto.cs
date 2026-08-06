using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de horário de funcionamento.</summary>
public class HorarioFuncionamentoDto
{
    public int Id { get; set; }
    public int PontoTuristicoId { get; set; }
    public DiaSemana DiaSemana { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFim { get; set; }
    public bool Fechado { get; set; }
}
