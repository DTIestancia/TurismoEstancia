using Microsoft.AspNetCore.Mvc.RazorPages;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.MidiaKit.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Pages.MidiaKit;

public class IndexModel : PageModel
{
    private readonly IMidiaKitService _midiaKit;

    public IndexModel(IMidiaKitService midiaKit) => _midiaKit = midiaKit;

    /// <summary>Materiais ativos com arquivo para download.</summary>
    public IReadOnlyList<MidiaKitItemDto> Itens { get; private set; } = Array.Empty<MidiaKitItemDto>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Itens = await _midiaKit.ListarAtivosAsync(ct);

        ViewData["Seo"] = new SeoMeta
        {
            Titulo = "Mídia kit",
            Descricao = "Materiais oficiais para imprensa e parceiros: logotipos, fotos e documentos do turismo de Estância."
        };
    }
}
