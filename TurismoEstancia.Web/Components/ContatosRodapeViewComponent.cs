using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Conteudo.Interfaces;

namespace TurismoEstancia.Web.Components;

/// <summary>
/// Renderiza os contatos do rodapé (endereços, telefones e redes sociais) com
/// os ícones corretos por tipo. Usado no rodapé da home e no das páginas
/// internas, para a marcação ficar idêntica em todo o portal.
/// </summary>
public class ContatosRodapeViewComponent : ViewComponent
{
    private const string CacheKey = "ContatosRodape__Lista";

    private readonly IContatoService _contatos;

    public ContatosRodapeViewComponent(IContatoService contatos) => _contatos = contatos;

    /// <summary>
    /// <paramref name="bloco"/>: "todos" (endereço + telefones + redes), ou um
    /// dos blocos isolados — "enderecos", "telefones", "redes".
    /// </summary>
    public async Task<IViewComponentResult> InvokeAsync(string bloco = "todos", CancellationToken ct = default)
    {
        // Cache por request: a home invoca o componente 3x (um bloco por coluna)
        // e as páginas internas 1x — sempre uma única consulta ao banco por página.
        if (HttpContext.Items.TryGetValue(CacheKey, out var cache) && cache is IReadOnlyList<ContatoDto> lista)
            return View(new ContatosRodapeModel(lista, bloco));

        var contatos = await _contatos.ListarAsync(null, ct);
        HttpContext.Items[CacheKey] = contatos;
        return View(new ContatosRodapeModel(contatos, bloco));
    }

    /// <summary>Dados exibidos na view do componente.</summary>
    public sealed record ContatosRodapeModel(IReadOnlyList<ContatoDto> Contatos, string Bloco);
}
