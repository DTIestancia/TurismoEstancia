namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Arquivo binário (imagem, vídeo, PDF) persistido em byte[] no banco,
/// servido pelo endpoint <c>GET /arquivo/{id}</c>. Nunca salvo no disco.
/// Estrutura alinhada ao padrão <c>PrefeituraDigital.Arquivo</c>: colunas
/// <c>ArquId / ArquUID / ArquFileName / ArquContentType / ArquSize /
/// ArquBytes / ArquMomento / ArquAutor / ArquAtivo / ArquOrigem</c>.
/// O <c>ArquBytes</c> é <c>varbinary(max)</c> e está pronto para virar
/// <c>FILESTREAM</c> quando o filegroup <c>FG_Arquivos_Stream</c> for
/// configurado no servidor (ver <c>Deploy/01-Filestream-Config.sql</c>).
/// A coluna <c>ArquUID</c> (uniqueidentifier) é o ROWGUIDCOL exigido pelo
/// FILESTREAM; sem ela, o SQL Server recusa a coluna FILESTREAM.
/// </summary>
public class Arquivo
{
    /// <summary>ArquId — chave primária (bigint identity).</summary>
    public long Id { get; set; }

    /// <summary>ArquUID — identificador único global (uniqueidentifier).
    /// ROWGUIDCOL obrigatório para o FILESTREAM; valor padrão NEWID().</summary>
    public Guid UID { get; set; } = Guid.NewGuid();

    /// <summary>ArquFileName — nome original do arquivo.</summary>
    public string Nome { get; set; } = null!;

    /// <summary>ArquContentType — Content-Type (ex.: image/jpeg, video/mp4, application/pdf).</summary>
    public string ContentType { get; set; } = null!;

    /// <summary>ArquSize — tamanho em bytes (equivalente ao DATALENGTH do binário).</summary>
    public long Size { get; set; }

    /// <summary>ArquBytes — binário <c>varbinary(max)</c>; vira FILESTREAM quando o filegroup for criado.</summary>
    public byte[] Bytes { get; set; } = null!;

    /// <summary>ArquAutor — usuário que gravou o arquivo (CMS/portal).</summary>
    public string? Autor { get; set; }

    /// <summary>ArquOrigem — origem do upload (seed, portal, gerenciador...).</summary>
    public string? Origem { get; set; }

    /// <summary>ArquAtivo — soft-delete.</summary>
    public bool Ativo { get; set; } = true;

    /// <summary>ArquMomento — data de gravação (GETDATE()).</summary>
    public DateTime CriadoEm { get; set; }
}
