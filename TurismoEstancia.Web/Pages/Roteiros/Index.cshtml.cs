using Microsoft.AspNetCore.Mvc.RazorPages;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Roteiro.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Pages.Roteiros;

public class IndexModel : PageModel
{
    private readonly IRoteiroService _roteiros;

    public IndexModel(IRoteiroService roteiros) => _roteiros = roteiros;

    public IReadOnlyList<RoteiroDto> Roteiros { get; private set; } = Array.Empty<RoteiroDto>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Roteiros = await _roteiros.ListarAsync(ct);
        ViewData["Seo"] = new SeoMeta
        {
            Titulo = "Roteiros Turísticos",
            Descricao = "Itinerários prontos para você viver o melhor de Estância: história, praias e natureza."
        };
    }
}
