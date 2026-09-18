using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Infra.Arquivos;

namespace TurismoEstancia.Services.Infra.Interfaces;

/// <summary>Serviço da tabela Arquivo (byte[] no banco, nunca no disco).</summary>
public interface IArquivoService
{
    /// <summary>
    /// Grava um upload e retorna o novo Id. Toda foto (JPEG, PNG, WebP, TIFF, BMP)
    /// entra otimizada pela regra única do sistema (ver <c>OtimizadorDeImagem</c>):
    /// 1600 px no maior lado, JPEG q82, rotação do EXIF aplicada aos pixels e
    /// metadados (EXIF/GPS) descartados — PNG com transparência continua PNG.
    /// Vídeo, PDF, SVG, .ico e GIF são guardados exatamente como vieram.
    ///
    /// É aqui que o limite por arquivo é aplicado (imagem 5 MB, vídeo 10 MB — ver
    /// <see cref="LimitesDeUpload"/>): acima do teto lança
    /// <see cref="InvalidOperationException"/> com o texto que o painel mostra ao
    /// operador. O limite vale na leitura dos bytes, antes de qualquer gravação.
    /// </summary>
    Task<long> SalvarAsync(IFormFile arquivo, CancellationToken ct = default);

    /// <summary>
    /// Grava bytes já em memória e retorna o novo Id, <b>sem</b> passar pela
    /// otimização. É o ponto de gravação cru: para upload de imagem use
    /// <see cref="SalvarAsync"/>, que já aplica a regra de otimização.
    /// </summary>
    Task<long> SalvarBytesAsync(string nome, string contentType, byte[] bytes, CancellationToken ct = default);

    /// <summary>
    /// Otimiza uma imagem (redimensiona para o máximo de <paramref name="maxDimensao"/>
    /// no maior lado, re-encoda como JPEG com a qualidade indicada e remove metadados
    /// EXIF) e grava na tabela Arquivo. Com <paramref name="comMarcaDagua"/> aplica a
    /// marca d'água do portal (listras diagonais + logotipo no canto). Retorna o novo Id.
    /// </summary>
    Task<long> SalvarImagemOtimizadaAsync(IFormFile arquivo, int maxDimensao = 1600, int qualidade = 82, bool comMarcaDagua = false, CancellationToken ct = default);

    /// <summary>Gera o thumbnail (400px) da imagem e grava na tabela Arquivo. Retorna o novo Id.</summary>
    Task<long> SalvarThumbnailAsync(IFormFile arquivo, int maxDimensao = 400, int qualidade = 75, CancellationToken ct = default);

    /// <summary>
    /// Favicon do site: redimensiona a imagem (nunca amplia) para caber em
    /// <paramref name="dimensao"/>px (padrão 64×64) e grava como PNG otimizado
    /// (sem metadados EXIF). Se o upload não for uma imagem decodificável
    /// (ex.: arquivo .ico), salva como está. Retorna o novo Id.
    /// </summary>
    Task<long> SalvarFaviconAsync(IFormFile arquivo, int dimensao = 64, CancellationToken ct = default);

    /// <summary>
    /// Gera um PNG (sem EXIF) a partir de um arquivo existente, redimensionado
    /// para caber em <paramref name="maxDimensao"/>px no maior lado (ex.: o
    /// apple-touch-icon 180×180). Retorna <c>null</c> se o arquivo não existir
    /// ou não for uma imagem decodificável.
    /// </summary>
    Task<byte[]?> GerarPngRedimensionadoAsync(long arquivoId, int maxDimensao, CancellationToken ct = default);

    /// <summary>Obtém o arquivo (com os bytes) para servir. Lança se não existir.</summary>
    Task<Arquivo> ObterAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Abre o binário para leitura direto do banco, com o tipo e o tamanho real do
    /// blob — <b>sem materializar o arquivo inteiro em memória e sem gravar nada em
    /// disco</b>: o fluxo devolvido busca o conteúdo em janelas conforme a leitura
    /// avança (leitura sequencial do <c>varbinary(max)</c>). É o caminho de toda
    /// mídia servida, vídeo inclusive, e o fluxo é pesquisável para o ASP.NET
    /// atender <c>Range</c>. Devolve <c>null</c> quando o arquivo não existe.
    /// </summary>
    Task<(Stream Fluxo, MetadadosDeArquivo Metadados)?> AbrirAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Gera a versão reduzida servida em <c>?largura=N</c> com a <b>mesma regra de imagem
    /// do upload</b> (ver <c>OtimizadorDeImagem</c>): nunca amplia, aplica a rotação do EXIF
    /// e descarta metadados, JPEG q82 (ou PNG quando a origem tem alfa). Retorna <c>null</c>
    /// quando o arquivo não existe, já é menor que o pedido — aí o original <i>é</i> a
    /// melhor versão — ou não é uma imagem redimensionável (GIF/APNG/WebP animado, SVG,
    /// vídeo etc.); nesses casos o chamador serve o original.
    /// </summary>
    Task<(byte[] Bytes, string ContentType, string Extensao)?> GerarRedimensionadoAsync(long arquivoId, int larguraMaxima, CancellationToken ct = default);

    /// <summary>Exclui o registro de arquivo — não faz nada se ainda estiver referenciado.</summary>
    Task ExcluirAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// True quando o arquivo é referenciado por <b>qualquer</b> entidade do sistema:
    /// todas as colunas que guardam id de arquivo, mais as seções que citam o id como
    /// texto (<c>historia-imagem</c> = "42", <c>/arquivo/42?largura=800</c>).
    /// </summary>
    Task<bool> EstaReferenciadoAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Todos os ids de arquivo citados em algum lugar do sistema, em uma consulta por
    /// fonte (e não uma por arquivo) — a base do relatório de órfãos.
    /// </summary>
    Task<IReadOnlySet<long>> IdsReferenciadosAsync(CancellationToken ct = default);

    /// <summary>Resumo de todo o acervo (sem os bytes), ordenado por id.</summary>
    Task<IReadOnlyList<ResumoDeArquivo>> ListarResumoAsync(CancellationToken ct = default);
}
