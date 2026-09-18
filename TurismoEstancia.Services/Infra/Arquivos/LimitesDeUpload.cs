using Microsoft.AspNetCore.Http;

namespace TurismoEstancia.Services.Infra.Arquivos;

/// <summary>
/// Limite de tamanho por arquivo enviado pelo painel: <b>imagem 5 MB, vídeo 10 MB</b>
/// (e 10 MB para o resto: PDF, SVG, ícone).
///
/// São <b>duas camadas</b>, e vale a menor:
/// <list type="number">
/// <item>o teto de transporte da requisição (Kestrel/IIS/multipart, ver
/// <c>InfrastructureExtensions</c>) — generoso de propósito, porque uma requisição
/// pode trazer VÁRIAS fotos de uma vez (a Galeria envia a seleção inteira num
/// formulário só);</item>
/// <item>este limite <i>por arquivo</i>, aplicado no ponto único de gravação
/// (<see cref="Interfaces.IArquivoService.SalvarAsync"/>), que é a regra que o
/// operador enxerga.</item>
/// </list>
///
/// Quando algum arquivo passa do teto, a validação lança
/// <see cref="InvalidOperationException"/> com o nome do arquivo, o tamanho dele e o
/// limite — é o texto que aparece no alerta do painel, porque todo controller de
/// upload já trata essa exceção e escreve em <c>TempData["PainelErro"]</c>.
/// </summary>
public static class LimitesDeUpload
{
    /// <summary>Teto de uma foto enviada pelo painel.</summary>
    public const long ImagemBytes = 5L * 1024 * 1024;

    /// <summary>Teto de um vídeo enviado pelo painel (hoje só o hero recebe vídeo).</summary>
    public const long VideoBytes = 10L * 1024 * 1024;

    /// <summary>Teto dos demais arquivos (guia em PDF, SVG, .ico) — igual ao do vídeo.</summary>
    public const long OutrosBytes = 10L * 1024 * 1024;

    /// <summary>
    /// Teto de transporte da requisição (Kestrel, IIS e multipart). É generoso de
    /// propósito: um formulário pode trazer VÁRIAS fotos de uma vez (a Galeria envia
    /// a seleção inteira) e cada uma tem seu próprio limite. É o que o
    /// <c>ErroDeUploadMiddleware</c> transforma em aviso quando estoura.
    /// </summary>
    public const long TetoDeTransporteBytes = 60L * 1024 * 1024;

    /// <summary>Categoria de arquivo, que define limite e texto da mensagem.</summary>
    private enum Categoria
    {
        Imagem,
        Video,
        Outro
    }

    /// <summary>Limite aplicável a um Content-Type declarado no upload.</summary>
    public static long Para(string? contentType) => De(contentType) switch
    {
        Categoria.Video => VideoBytes,
        Categoria.Imagem => ImagemBytes,
        _ => OutrosBytes
    };

    /// <summary>
    /// Como o tipo é chamado no início da mensagem de erro — com o artigo certo
    /// ("A imagem", "O vídeo", "O arquivo").
    /// </summary>
    public static string Descrever(string? contentType) => De(contentType) switch
    {
        Categoria.Video => "O vídeo",
        Categoria.Imagem => "A imagem",
        _ => "O arquivo"
    };

    /// <summary>
    /// Barra o upload que passou do teto antes de qualquer leitura dos bytes.
    /// </summary>
    public static void Validar(IFormFile arquivo)
    {
        if (arquivo is null || arquivo.Length == 0)
            return;

        var categoria = De(arquivo.ContentType);
        var limite = Para(arquivo.ContentType);
        if (arquivo.Length <= limite)
            return;

        throw new InvalidOperationException(
            $"{Descrever(arquivo.ContentType)} \"{Path.GetFileName(arquivo.FileName)}\" tem " +
            $"{Mb(arquivo.Length)} MB e o limite é {Mb(limite)} MB. {Dica(categoria)}");
    }

    /// <summary>O que fazer para caber no limite — muda conforme o tipo.</summary>
    private static string Dica(Categoria categoria) => categoria switch
    {
        Categoria.Imagem =>
            "Salve a foto em JPEG com até 1600 px no maior lado (o portal não exibe maior que isso) e envie de novo.",
        Categoria.Video =>
            "Exporte o vídeo em H.264 (MP4) 1080p ou 720p — 10 MB cobrem cerca de 30 segundos.",
        _ => "Envie um arquivo menor."
    };

    private static Categoria De(string? contentType)
    {
        var tipo = (contentType ?? string.Empty).Split(';', 2)[0].Trim();

        if (tipo.StartsWith("video/", StringComparison.OrdinalIgnoreCase)) return Categoria.Video;
        if (tipo.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) return Categoria.Imagem;
        return Categoria.Outro;
    }

    /// <summary>Tamanho em MB com uma casa decimal (pt-BR), para as mensagens do painel.</summary>
    public static string Mb(long bytes) =>
        (bytes / 1024d / 1024d).ToString("N1", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));

    /// <summary>Limite em MB sem casa decimal quando é redondo ("10 MB", "5 MB") — para as telas.</summary>
    public static string Rotulo(long bytes) => $"{bytes / 1024d / 1024d:0.#} MB";
}
