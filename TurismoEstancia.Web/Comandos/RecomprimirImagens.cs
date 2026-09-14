using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Web.Comandos;

/// <summary>
/// Comando de manutenção do acervo de imagens: aplica em toda foto já gravada a
/// mesma otimização que o upload faz hoje (1600 px, JPEG, sem EXIF) e mostra o
/// antes/depois em bytes.
///
/// Uso (na pasta do deploy, com o portal parado de preferência):
/// <code>
/// dotnet TurismoEstancia.Web.dll recomprimir-imagens             # aplica
/// dotnet TurismoEstancia.Web.dll recomprimir-imagens --simular   # só mede
/// dotnet run --project TurismoEstancia.Web -- recomprimir-imagens --simular
/// </code>
///
/// Rodar duas vezes é seguro: a segunda passada não encontra nada para fazer.
/// </summary>
public static class RecomprimirImagens
{
    public const string Nome = "recomprimir-imagens";

    /// <summary>True quando a linha de comando pediu este comando.</summary>
    public static bool EhComando(string[] args) =>
        args.Length > 0 && string.Equals(args[0], Nome, StringComparison.OrdinalIgnoreCase);

    public static async Task<int> ExecutarAsync(WebApplication app, string[] args)
    {
        var simular = args.Any(a => string.Equals(a, "--simular", StringComparison.OrdinalIgnoreCase));

        Console.WriteLine(simular
            ? "SIMULAÇÃO: as fotos são medidas, mas nada é gravado."
            : "Recomprimindo o acervo. Nenhuma foto aumenta de tamanho e nada é apagado.");

        using var escopo = app.Services.CreateScope();
        var recompressor = escopo.ServiceProvider.GetRequiredService<IRecompressorImagensService>();

        var resultado = await recompressor.ExecutarAsync(simular, RelatarProgresso, CancellationToken.None);
        ImprimirRelatorio(resultado);
        return 0;
    }

    private static void RelatarProgresso(RecompressaoImagensDto parcial) =>
        Console.WriteLine(
            $"  ... {parcial.Analisados:N0} fotos analisadas (otimizadas: {parcial.Otimizados:N0}) — " +
            $"{Bytes(parcial.BytesAntes)} -> {Bytes(parcial.BytesDepois)}");

    private static void ImprimirRelatorio(RecompressaoImagensDto r)
    {
        Console.WriteLine();
        Console.WriteLine("==================== Recompressão do acervo ====================");
        Console.WriteLine($"Fotos no escopo .............................. {r.Analisados,10:N0}");
        Console.WriteLine($"  otimizadas (ficaram menores) ............... {r.Otimizados,10:N0}");
        Console.WriteLine($"  já otimizadas (nada a fazer) ............... {r.JaOtimizados,10:N0}");
        Console.WriteLine($"  sem ganho (original mantido) ............... {r.SemGanho,10:N0}");
        Console.WriteLine($"  não decodificadas .......................... {r.NaoDecodificados,10:N0}");
        Console.WriteLine($"  com falha .................................. {r.ComFalha,10:N0}");
        Console.WriteLine($"Fora do escopo (vídeo, PDF, SVG, GIF) ........ {r.ForaDoEscopo,10:N0}");
        Console.WriteLine();
        Console.WriteLine($"Bytes das fotos: {Bytes(r.BytesAntes)} -> {Bytes(r.BytesDepois)}  ({Porcentagem(r)})");

        if (r.Simulado)
        {
            Console.WriteLine("Miniaturas derivadas: preservadas (simulação).");
        }
        else
        {
            Console.WriteLine($"Miniaturas derivadas descartadas: {r.MiniaturasRemovidas:N0} ({Bytes(r.MiniaturasBytesRemovidos)})");
        }

        if (r.MaioresGanhos.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Maiores ganhos:");
            foreach (var ganho in r.MaioresGanhos.OrderByDescending(g => g.Economia).Take(10))
            {
                var percentual = ganho.BytesAntes == 0 ? 0 : (int)Math.Round(100.0 * ganho.Economia / ganho.BytesAntes);
                Console.WriteLine($"  #{ganho.ArquivoId,-7} {ganho.Nome,-38} {Bytes(ganho.BytesAntes),10} -> {Bytes(ganho.BytesDepois),10}  (-{percentual}%)");
            }
        }

        Console.WriteLine();
        if (r.Simulado)
        {
            Console.WriteLine("Simulação: rode sem --simular para aplicar.");
        }
        else
        {
            Console.WriteLine("Aplicado. O navegador baixa as imagens de novo (o ETag muda) e as");
            Console.WriteLine("miniaturas ?largura=N são geradas outra vez no primeiro acesso.");
        }
    }

    private static string Porcentagem(RecompressaoImagensDto r)
    {
        if (r.BytesAntes <= 0) return "sem fotos no escopo";
        var economia = r.BytesAntes - r.BytesDepois;
        var percentual = Math.Round(100.0 * economia / r.BytesAntes, 1);
        return $"{percentual:N1}% menor ({(economia < 0 ? "+" : "-")}{Bytes(Math.Abs(economia))})";
    }

    /// <summary>Tamanho legível (B, KB, MB, GB) — usado também nas linhas de progresso.</summary>
    private static string Bytes(long bytes)
    {
        string[] unidades = { "B", "KB", "MB", "GB", "TB" };
        double valor = bytes;
        var unidade = 0;
        while (valor >= 1024 && unidade < unidades.Length - 1)
        {
            valor /= 1024;
            unidade++;
        }

        return unidade == 0
            ? $"{bytes} B"
            : $"{valor.ToString("N1", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"))} {unidades[unidade]}";
    }
}
