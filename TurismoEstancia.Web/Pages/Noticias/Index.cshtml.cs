using Microsoft.AspNetCore.Mvc.RazorPages;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Web.Infrastructure;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Pages.Noticias;

public class IndexModel : PageModel
{
    private readonly INoticiaService _noticias;
    private readonly IConteudoSiteService _conteudos;

    public IndexModel(INoticiaService noticias, IConteudoSiteService conteudos)
    {
        _noticias = noticias;
        _conteudos = conteudos;
    }

    /// <summary>Textos do portal (inclui a capa gerenciável noticias-capa).</summary>
    public Dictionary<string, string?> Conteudos { get; private set; } = new();

    /// <summary>Notícias publicadas da página atual (12 por página).</summary>
    public IReadOnlyList<NoticiaDto> Noticias { get; private set; } = Array.Empty<NoticiaDto>();

    /// <summary>Total real de notícias publicadas.</summary>
    public int TotalNoticias { get; private set; }

    public int PaginaAtual { get; private set; } = 1;

    public int PaginasTotal { get; private set; } = 1;

    public async Task OnGetAsync(CancellationToken ct, int pagina = 1)
    {
        Conteudos = await _conteudos.ObterDicionarioAsync(ct);
        var todas = await _noticias.ListarAsync(apenasPublicadas: true, ct);
        TotalNoticias = todas.Count;
        PaginasTotal = Math.Max(1, (int)Math.Ceiling(TotalNoticias / (double)PaginaService.Tamanho));
        PaginaAtual = Math.Clamp(pagina, 1, PaginasTotal);
        Noticias = todas.Skip((PaginaAtual - 1) * PaginaService.Tamanho).Take(PaginaService.Tamanho).ToList();

        ViewData["Seo"] = new SeoMeta
        {
            Titulo = "Notícias",
            Descricao = "Cultura, eventos e novidades de Estância — Capital Sergipana da Cultura."
        };
    }
}

