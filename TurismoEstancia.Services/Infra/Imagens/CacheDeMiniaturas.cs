namespace TurismoEstancia.Services.Infra.Imagens;

/// <summary>
/// Cache em disco das miniaturas derivadas de <c>?largura=N</c> (uma por arquivo e
/// largura, fora do wwwroot de propósito). Mora aqui porque dois lugares precisam
/// apontar para a MESMA pasta: o endpoint que serve a imagem e o comando de
/// manutenção, que precisa descartar as miniaturas antigas depois de recomprimir o
/// acervo (senão um arquivo de 1920 px de uma foto que agora tem 1600 px continuaria
/// sendo servido para sempre).
/// </summary>
public static class CacheDeMiniaturas
{
    /// <summary>Pasta do cache de miniaturas para um ContentRoot.</summary>
    public static string Pasta(string contentRoot) => Path.Combine(contentRoot, "cache", "arquivo");

    /// <summary>
    /// Apaga as miniaturas derivadas. Devolve quantos arquivos e quantos bytes saíram.
    /// Arquivo em uso (IIS com o portal no ar) é ignorado: o próximo acesso regenera.
    /// </summary>
    public static (int Arquivos, long Bytes) Limpar(string contentRoot)
    {
        var pasta = Pasta(contentRoot);
        if (!Directory.Exists(pasta)) return (0, 0);

        var arquivos = 0;
        var bytes = 0L;
        foreach (var caminho in Directory.EnumerateFiles(pasta))
        {
            try
            {
                bytes += new FileInfo(caminho).Length;
                File.Delete(caminho);
                arquivos++;
            }
            catch (IOException) { /* em uso: fica para o próximo deploy */ }
            catch (UnauthorizedAccessException) { /* idem */ }
        }

        return (arquivos, bytes);
    }
}
