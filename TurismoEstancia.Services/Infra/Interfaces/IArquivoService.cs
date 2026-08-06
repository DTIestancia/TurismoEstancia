using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Services.Infra.Interfaces;

/// <summary>Serviço da tabela Arquivo (byte[] no banco, nunca no disco).</summary>
public interface IArquivoService
{
    /// <summary>Grava um upload em byte[] e retorna o novo Id.</summary>
    Task<long> SalvarAsync(IFormFile arquivo, CancellationToken ct = default);

    /// <summary>Grava bytes já em memória e retorna o novo Id.</summary>
    Task<long> SalvarBytesAsync(string nome, string contentType, byte[] bytes, CancellationToken ct = default);

    /// <summary>Obtém o arquivo para servir com Content-Type correto. Lança se não existir.</summary>
    Task<Arquivo> ObterAsync(long id, CancellationToken ct = default);

    /// <summary>Exclui o registro de arquivo (usado para limpar órfãos).</summary>
    Task ExcluirAsync(long id, CancellationToken ct = default);

    /// <summary>True quando o arquivo é referenciado por alguma entidade.</summary>
    Task<bool> EstaReferenciadoAsync(long id, CancellationToken ct = default);
}
