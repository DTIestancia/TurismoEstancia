using Microsoft.AspNetCore.Mvc.RazorPages;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Turismo.Interfaces;
using TurismoEstancia.Web.Infrastructure;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Pages.Agenda;

public class IndexModel : PageModel
{
    private readonly IEventoService _eventos;

    public IndexModel(IEventoService eventos) => _eventos = eventos;

    /// <summary>Eventos futuros da página atual (12 por página).</summary>
    public IReadOnlyList<EventoDto> Eventos { get; private set; } = Array.Empty<EventoDto>();

    public int TotalEventos { get; private set; }

    public int PaginaAtual { get; private set; } = 1;

    public int PaginasTotal { get; private set; } = 1;

    /// <summary>Eventos já encerrados da página atual (12 por página, mais recentes primeiro).</summary>
    public IReadOnlyList<EventoDto> EventosPassados { get; private set; } = Array.Empty<EventoDto>();

    public int TotalEventosPassados { get; private set; }

    public int PaginaPassadosAtual { get; private set; } = 1;

    public int PaginasTotalPassados { get; private set; } = 1;

    public async Task OnGetAsync(CancellationToken ct, int pagina = 1, int paginaPassados = 1)
    {
        var todos = await _eventos.ListarAsync(apenasProximos: false, ct);
        var hoje = DateTime.Today;

        var proximos = todos.Where(e => e.DataFim >= hoje).ToList();
        var passados = todos.Where(e => e.DataFim < hoje).OrderByDescending(e => e.DataInicio).ToList();

        TotalEventos = proximos.Count;
        PaginasTotal = Math.Max(1, (int)Math.Ceiling(TotalEventos / (double)PaginaService.Tamanho));
        PaginaAtual = Math.Clamp(pagina, 1, PaginasTotal);
        Eventos = proximos.Skip((PaginaAtual - 1) * PaginaService.Tamanho).Take(PaginaService.Tamanho).ToList();

        TotalEventosPassados = passados.Count;
        PaginasTotalPassados = Math.Max(1, (int)Math.Ceiling(TotalEventosPassados / (double)PaginaService.Tamanho));
        PaginaPassadosAtual = Math.Clamp(paginaPassados, 1, PaginasTotalPassados);
        EventosPassados = passados.Skip((PaginaPassadosAtual - 1) * PaginaService.Tamanho).Take(PaginaService.Tamanho).ToList();

        ViewData["Seo"] = new SeoMeta
        {
            Titulo = "Agenda de Eventos",
            Descricao = "Programação oficial de eventos em Estância — Capital Sergipana da Cultura."
        };
    }
}
