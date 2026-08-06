using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Turismo.Interfaces;

/// <summary>Serviço de eventos da agenda (com geração de arquivo .ics).</summary>
public interface IEventoService
{
    /// <summary>Lista eventos; <paramref name="apenasProximos"/> filtra os já encerrados (portal).</summary>
    Task<IReadOnlyList<EventoDto>> ListarAsync(bool apenasProximos = false, CancellationToken ct = default);

    Task<EventoDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

    Task SalvarAsync(EventoDto dto, CancellationToken ct = default);

    Task ExcluirAsync(int id, CancellationToken ct = default);

    /// <summary>Gera o conteúdo do arquivo .ics do evento para importação em calendários.</summary>
    Task<string> GerarIcsAsync(int id, CancellationToken ct = default);
}
