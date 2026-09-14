using Microsoft.AspNetCore.Mvc.RazorPages;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Services.Planeje.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Pages.Roteiros;

/// <summary>
/// Página "Planeje sua viagem" — mesmo conteúdo da seção da home (categorias
/// e itens do Planeje, com filtros, modal e avaliações). Os antigos roteiros
/// de itinerário (dia a dia) seguem no Gerenciador, sem link no menu.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IPlanejeService _planeje;
    private readonly IConteudoSiteService _conteudos;

    public IndexModel(IPlanejeService planeje, IConteudoSiteService conteudos)
    {
        _planeje = planeje;
        _conteudos = conteudos;
    }

    public Dictionary<string, string?> Conteudos { get; private set; } = new();

    public IReadOnlyList<PlanejeCategoriaDto> PlanejeCategorias { get; private set; } = Array.Empty<PlanejeCategoriaDto>();

    public IReadOnlyList<PlanejeItemDto> PlanejeItens { get; private set; } = Array.Empty<PlanejeItemDto>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Conteudos = await _conteudos.ObterDicionarioAsync(ct);
        PlanejeCategorias = await _planeje.ListarCategoriasAsync(true, ct);
        PlanejeItens = await _planeje.ListarItensAsync(true, ct);
        ViewData["Seo"] = new SeoMeta
        {
            Titulo = "Planeje sua viagem",
            Descricao = "Onde ficar, onde comer e serviços para você viver o melhor de Estância."
        };
    }
}
