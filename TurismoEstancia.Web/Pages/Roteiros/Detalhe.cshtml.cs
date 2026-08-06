using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Roteiro.Interfaces;

namespace TurismoEstancia.Web.Pages.Roteiros;

public class DetalheModel : PageModel
{
    private readonly IRoteiroService _roteiros;

    public DetalheModel(IRoteiroService roteiros) => _roteiros = roteiros;

    public RoteiroDto? Roteiro { get; private set; }

    /// <summary>Itens agrupados por dia (ordem natural dos dias).</summary>
    public IReadOnlyList<IGrouping<int, RoteiroItemDto>> Dias { get; private set; } = Array.Empty<IGrouping<int, RoteiroItemDto>>();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        Roteiro = await _roteiros.ObterPorIdAsync(id, ct);
        if (Roteiro is null) return NotFound();

        Dias = Roteiro.Itens
            .OrderBy(i => i.Dia)
            .ThenBy(i => i.Ordem)
            .GroupBy(i => i.Dia)
            .ToList();

        return Page();
    }
}
