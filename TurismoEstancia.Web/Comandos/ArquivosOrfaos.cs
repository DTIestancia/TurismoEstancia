using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Web.Comandos;

/// <summary>
/// Comando de manutenção do acervo: lista os arquivos gravados que <b>ninguém usa</b>
/// (nem por coluna de id, nem citados como texto numa seção do portal) e mostra
/// quanto de espaço eles ocupam.
///
/// Uso (na pasta do deploy):
/// <code>
/// dotnet TurismoEstancia.Web.dll arquivos-orfaos            # só relata
/// dotnet TurismoEstancia.Web.dll arquivos-orfaos --excluir  # relata e apaga
/// dotnet run --project TurismoEstancia.Web -- arquivos-orfaos
/// </code>
///
/// O relatório é conservador por construção: a regra é a mesma que protege a exclusão
/// no painel, e a remoção revalida cada arquivo antes de apagar. Na dúvida, não apaga.
/// </summary>
public static class ArquivosOrfaos
{
    public const string Nome = "arquivos-orfaos";

    /// <summary>True quando a linha de comando pediu este comando.</summary>
    public static bool EhComando(string[] args) =>
        args.Length > 0 && string.Equals(args[0], Nome, StringComparison.OrdinalIgnoreCase);

    public static async Task<int> ExecutarAsync(WebApplication app, string[] args)
    {
        var excluir = args.Any(a => string.Equals(a, "--excluir", StringComparison.OrdinalIgnoreCase));

        Console.WriteLine(excluir
            ? "Levantando os órfãos e REMOVENDO o que não é usado por ninguém."
            : "Levantando os órfãos (nada será apagado — use --excluir para remover).");

        using var escopo = app.Services.CreateScope();
        var auditoria = escopo.ServiceProvider.GetRequiredService<IAuditoriaDeArquivosService>();

        var resultado = await auditoria.ExecutarAsync(excluir, CancellationToken.None);
        ImprimirRelatorio(resultado);
        return 0;
    }

    private static void ImprimirRelatorio(OrfaosDeArquivoDto r)
    {
        Console.WriteLine();
        Console.WriteLine("==================== Arquivos órfãos ====================");
        Console.WriteLine($"Arquivos no acervo ........................... {r.Total,10:N0}");
        Console.WriteLine($"  em uso .................................... {r.Referenciados,10:N0}");
        Console.WriteLine($"  SEM uso (órfãos) .......................... {r.Itens.Count + r.ItensOmitidos,10:N0}");
        Console.WriteLine();
        Console.WriteLine($"Tamanho do acervo ............................ {Bytes(r.BytesTotal),10}");
        Console.WriteLine($"Tamanho dos órfãos ........................... {Bytes(r.BytesOrfaos),10}");

        if (r.Itens.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Maiores órfãos:");
            foreach (var item in r.Itens)
            {
                Console.WriteLine($"  #{item.ArquivoId,-7} {Recortar(item.Nome, 36),-36} {item.ContentType,-22} " +
                                  $"{Bytes(item.Size),10}  {item.CriadoEm:dd/MM/yyyy}  {item.Autor ?? "-"}");
            }

            if (r.ItensOmitidos > 0)
                Console.WriteLine($"  ... e mais {r.ItensOmitidos:N0} arquivos menores (o relatório mostra os 25 maiores).");
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("Nenhum órfão: todo arquivo do acervo está em uso.");
        }

        Console.WriteLine();
        if (!r.Excluido)
        {
            Console.WriteLine("Nada foi apagado. Para remover os órfãos, rode com --excluir.");
        }
        else
        {
            Console.WriteLine($"Removidos: {r.Excluidos:N0}" + (r.ComFalha > 0 ? $"  (falhas: {r.ComFalha:N0} — ver o log)" : ""));
            Console.WriteLine("Nada em disco foi tocado: as versões reduzidas são derivadas na memória do portal.");
        }
    }

    private static string Recortar(string texto, int limite) =>
        texto.Length <= limite ? texto : texto[..(limite - 1)] + "…";

    /// <summary>Tamanho legível (B, KB, MB, GB).</summary>
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
