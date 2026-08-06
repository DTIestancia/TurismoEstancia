using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Services.Turismo.Services;

/// <summary>Implementação do serviço de eventos.</summary>
public class EventoService : IEventoService
{
    private readonly AppDbContext _db;

    public EventoService(AppDbContext db) => _db = db;

    private static readonly Expression<Func<Evento, EventoDto>> ToDto =
        e => new EventoDto
        {
            Id = e.Id,
            Titulo = e.Titulo,
            Descricao = e.Descricao,
            Local = e.Local,
            DataInicio = e.DataInicio,
            DataFim = e.DataFim,
            Ordem = e.Ordem,
            Ativo = e.Ativo
        };

    public async Task<IReadOnlyList<EventoDto>> ListarAsync(bool apenasProximos = false, CancellationToken ct = default)
    {
        var hoje = DateTime.Today;
        return await _db.Eventos.AsNoTracking()
            .Where(e => e.Ativo && (!apenasProximos || e.DataFim >= hoje))
            .OrderBy(e => e.DataInicio)
            .ThenBy(e => e.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);
    }

    public async Task<EventoDto?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        await _db.Eventos.AsNoTracking()
            .Where(e => e.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);

    public async Task SalvarAsync(EventoDto dto, CancellationToken ct = default)
    {
        if (dto.Id == 0)
        {
            _db.Eventos.Add(new Evento
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Local = dto.Local,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                Ordem = dto.Ordem,
                Ativo = true
            });
        }
        else
        {
            var entidade = await _db.Eventos.FirstOrDefaultAsync(e => e.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Evento não encontrado.");
            entidade.Titulo = dto.Titulo;
            entidade.Descricao = dto.Descricao;
            entidade.Local = dto.Local;
            entidade.DataInicio = dto.DataInicio;
            entidade.DataFim = dto.DataFim;
            entidade.Ordem = dto.Ordem;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var entidade = await _db.Eventos.FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new InvalidOperationException("Evento não encontrado.");
        _db.Eventos.Remove(entidade);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<string> GerarIcsAsync(int id, CancellationToken ct = default)
    {
        var evento = await _db.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new InvalidOperationException("Evento não encontrado.");

        var agora = DateTime.Now;
        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//TurismoEstancia//PT-BR//");
        sb.AppendLine("BEGIN:VEVENT");
        sb.AppendLine($"UID:{evento.Id}@turismoestancia.com.br");
        sb.AppendLine($"DTSTAMP:{agora:yyyyMMddTHHmmssZ}");
        sb.AppendLine($"DTSTART:{evento.DataInicio:yyyyMMddTHHmmss}");
        sb.AppendLine($"DTEND:{evento.DataFim:yyyyMMddTHHmmss}");
        sb.AppendLine($"SUMMARY:{EscapeIcs(evento.Titulo)}");
        if (!string.IsNullOrWhiteSpace(evento.Descricao))
            sb.AppendLine($"DESCRIPTION:{EscapeIcs(evento.Descricao)}");
        if (!string.IsNullOrWhiteSpace(evento.Local))
            sb.AppendLine($"LOCATION:{EscapeIcs(evento.Local)}");
        sb.AppendLine("END:VEVENT");
        sb.AppendLine("END:VCALENDAR");
        return sb.ToString();
    }

    private static string EscapeIcs(string texto) =>
        texto.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\n", "\\n");
}
