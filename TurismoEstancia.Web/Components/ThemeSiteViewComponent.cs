using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Text;
using TurismoEstancia.Services.Conteudo.Interfaces;

namespace TurismoEstancia.Web.Components;

/// <summary>
/// Tema do portal: as 6 cores da paleta ficam em configurações do Gerenciador
/// (chaves <c>tema-cor-*</c>, tipo Texto, valor em hex). Quando alguma cor está
/// configurada, este componente emite um <c>&lt;style&gt;</c> que sobrescreve as
/// variáveis CSS <c>:root</c> em tempo de execução — sem recompilar o SCSS.
/// As variantes da escala (escuros/claros e alfas) são derivadas da cor base
/// com <c>color-mix()</c>; famílias sem configuração mantêm o valor compilado.
/// </summary>
public class ThemeSiteViewComponent : ViewComponent
{
    // Chaves das configurações (tipo Texto, valor #rrggbb).
    public const string ChaveVermelho = "tema-cor-vermelho";
    public const string ChaveLaranja = "tema-cor-laranja";
    public const string ChaveAmarelo = "tema-cor-amarelo";
    public const string ChaveVerde = "tema-cor-verde";
    public const string ChaveAzul = "tema-cor-azul";
    public const string ChaveRosa = "tema-cor-rosa";

    /// <summary>Padrão oficial exibido quando nada foi configurado (fallback do SCSS).</summary>
    public static readonly IReadOnlyList<(string Chave, string Nome, string Variavel, string Padrao)> Cores =
    [
        (ChaveVermelho, "Vermelho", "--color-red-54", "#ED2027"),
        (ChaveLaranja, "Laranja", "--color-orange-48", "#F97E31"),
        (ChaveAmarelo, "Amarelo", "--color-yellow-50", "#FCBB0F"),
        (ChaveVerde, "Verde", "--color-spring-green-29", "#658746"),
        (ChaveAzul, "Azul", "--color-cyan-41", "#0095F6"),
        (ChaveRosa, "Rosa", "--color-rose-46", "#E9568A")
    ];

    private readonly IConfiguracaoSiteService _configuracoes;

    public ThemeSiteViewComponent(IConfiguracaoSiteService configuracoes) => _configuracoes = configuracoes;

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken ct = default)
    {
        var configuracoes = new Dictionary<string, string>();
        foreach (var cor in Cores)
        {
            var cfg = await _configuracoes.ObterPorChaveAsync(cor.Chave, ct);
            if (cfg?.ValorTexto is { Length: 7 } hex && hex[0] == '#' && EhHex(hex))
                configuracoes[cor.Variavel] = hex;
        }

        if (configuracoes.Count == 0)
            return Content(string.Empty);

        // HtmlContentViewComponentResult escreve o HTML sem encode — o
        // Content() padrão escaparia as aspas e o <style> viraria texto.
        return new HtmlContentViewComponentResult(
            new HtmlString($"<style id=\"tema-site\">{MontarCss(configuracoes)}</style>"));
    }

    private static bool EhHex(string valor) =>
        valor[1..].All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F');

    /// <summary>
    /// Monta o bloco <c>:root</c> com a cor base de cada família configurada e as
    /// variantes derivadas (escala de leveza e alfas) usando color-mix().
    /// </summary>
    private static string MontarCss(IReadOnlyDictionary<string, string> cores)
    {
        var sb = new StringBuilder(":root{");

        if (cores.TryGetValue("--color-red-54", out var vermelho))
        {
            sb.Append("--color-red-54:").Append(vermelho).Append(';')
              .Append("--color-red-49:color-mix(in srgb,").Append(vermelho).Append(" 88%,#fff);")
              .Append("--color-red-43-40:color-mix(in srgb,").Append(vermelho).Append(" 40%,transparent);")
              .Append("--color-red-43-0:color-mix(in srgb,").Append(vermelho).Append(" 0%,transparent);");
        }

        if (cores.TryGetValue("--color-orange-48", out var laranja))
        {
            sb.Append("--color-orange-48:").Append(laranja).Append(';')
              .Append("--color-orange-34:color-mix(in srgb,").Append(laranja).Append(" 55%,#000);")
              .Append("--color-orange-35:color-mix(in srgb,").Append(laranja).Append(" 58%,#000);")
              .Append("--color-orange-47:color-mix(in srgb,").Append(laranja).Append(" 80%,#000);")
              .Append("--color-orange-51:color-mix(in srgb,").Append(laranja).Append(" 88%,#000);")
              .Append("--color-orange-57:color-mix(in srgb,").Append(laranja).Append(" 78%,#fff);")
              .Append("--color-orange-48-10:color-mix(in srgb,").Append(laranja).Append(" 10%,transparent);")
              .Append("--color-orange-48-20:color-mix(in srgb,").Append(laranja).Append(" 20%,transparent);");
        }

        if (cores.TryGetValue("--color-yellow-50", out var amarelo))
            sb.Append("--color-yellow-50:").Append(amarelo).Append(';');

        if (cores.TryGetValue("--color-spring-green-29", out var verde))
            sb.Append("--color-spring-green-29:").Append(verde).Append(';');

        if (cores.TryGetValue("--color-cyan-41", out var azul))
            sb.Append("--color-cyan-41:").Append(azul).Append(';');

        if (cores.TryGetValue("--color-rose-46", out var rosa))
        {
            sb.Append("--color-rose-46:").Append(rosa).Append(';')
              .Append("--color-rose-42:color-mix(in srgb,").Append(rosa).Append(" 62%,#000);")
              .Append("--color-rose-42-0:color-mix(in srgb,").Append(rosa).Append(" 0%,transparent);");
        }

        sb.Append('}');
        return sb.ToString();
    }
}
