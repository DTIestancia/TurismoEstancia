using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TurismoEstancia.Web.Models;

/// <summary>Gera slugs (URLs amigáveis) a partir de nomes — ex.: "Praia do Saco" → "praia-do-saco".</summary>
public static class Slug
{
    public static string De(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";

        // Remove acentos (FormD separa os diacríticos, que são descartados).
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizado.Length);
        foreach (var ch in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        var limpo = sb.ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();

        limpo = Regex.Replace(limpo, "[^a-z0-9]+", "-").Trim('-');
        return limpo;
    }
}
