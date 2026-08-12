using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Services.Infra.Interfaces;

/// <summary>Serviço da tabela Arquivo (byte[] no banco, nunca no disco).</summary>
public interface IArquivoService
{
    /// <summary>Grava um upload em byte[] e retorna o novo Id.</summary>
    Task<long> SalvarAsync(IFormFile arquivo, CancellationToken ct = default);

    /// <summary>Grava bytes já em memória e retorna o novo Id.</summary>
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

    /// <summary>Obtém o arquivo para servir com Content-Type correto. Lança se não existir.</summary>
    Task<Arquivo> ObterAsync(long id, CancellationToken ct = default);

    /// <summary>Exclui o registro de arquivo (usado para limpar órfãos).</summary>
    Task ExcluirAsync(long id, CancellationToken ct = default);

    /// <summary>True quando o arquivo é referenciado por alguma entidade.</summary>
    Task<bool> EstaReferenciadoAsync(long id, CancellationToken ct = default);
}
