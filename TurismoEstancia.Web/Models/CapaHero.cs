using System.Globalization;

namespace TurismoEstancia.Web.Models;

/// <summary>Capa do hero de uma página interna (foto + recorte gerenciáveis).</summary>
public sealed class CapaHero
{
    private CapaHero() { }

    public bool Tem { get; private init; }
    public long Id { get; private init; }
    public int Zoom { get; private init; } = 100;
    public int PosX { get; private init; } = 50;
    public int PosY { get; private init; } = 50;

    /// <summary>URL da capa (miniatura 1920px) ou vazio.</summary>
    public string Url { get; private init; } = string.Empty;

    /// <summary>Estilo do &lt;img&gt; (posição + zoom com origem no foco).</summary>
    public string Estilo =>
        $"object-position:{PosX}% {PosY}%;transform-origin:{PosX}% {PosY}%;transform:scale({(Zoom / 100.0).ToString(CultureInfo.InvariantCulture)});";

    /// <summary>
    /// Monta a capa a partir do dicionário de conteúdos (<c>{base}</c>,
    /// <c>{base}-zoom</c>, <c>{base}-pos-x</c>, <c>{base}-pos-y</c>).
    /// O <paramref name="urlBase"/> é prefixado ao caminho do arquivo.
    /// </summary>
    public static CapaHero Montar(IReadOnlyDictionary<string, string?> conteudos, string chaveBase, Func<long, string> urlBase)
    {
        if (!conteudos.TryGetValue(chaveBase, out var texto)
            || !long.TryParse(texto, out var id)
            || id <= 0)
        {
            return new CapaHero();
        }

        static int Ler(IReadOnlyDictionary<string, string?> d, string chave, int padrao, int min, int max) =>
            d.TryGetValue(chave, out var t) && int.TryParse(t, out var v) ? Math.Clamp(v, min, max) : padrao;

        return new CapaHero
        {
            Tem = true,
            Id = id,
            Zoom = Ler(conteudos, $"{chaveBase}-zoom", 100, 100, 250),
            PosX = Ler(conteudos, $"{chaveBase}-pos-x", 50, 0, 100),
            PosY = Ler(conteudos, $"{chaveBase}-pos-y", 50, 0, 100),
            Url = urlBase(id)
        };
    }
}
