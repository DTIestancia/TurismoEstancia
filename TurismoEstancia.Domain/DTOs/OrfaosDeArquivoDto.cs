namespace TurismoEstancia.Domain.DTOs;

/// <summary>
/// Resultado da auditoria do acervo: o que está gravado na tabela Arquivo e
/// <b>não é usado por ninguém</b> (nem por coluna de id, nem por citação em texto).
/// </summary>
public class OrfaosDeArquivoDto
{
    /// <summary>Arquivos no acervo.</summary>
    public int Total { get; set; }

    /// <summary>Quantos estão em uso.</summary>
    public int Referenciados { get; set; }

    /// <summary>Bytes do acervo inteiro.</summary>
    public long BytesTotal { get; set; }

    /// <summary>Bytes que estão sobrando.</summary>
    public long BytesOrfaos { get; set; }

    /// <summary>True quando o comando foi rodado com <c>--excluir</c>.</summary>
    public bool Excluido { get; set; }

    /// <summary>Quantos órfãos foram removidos (só com <c>--excluir</c>).</summary>
    public int Excluidos { get; set; }

    /// <summary>Quantos não puderam ser removidos.</summary>
    public int ComFalha { get; set; }

    /// <summary>Os maiores órfãos, para o relatório (ver <see cref="ItensOmitidos"/>).</summary>
    public List<OrfaoItemDto> Itens { get; set; } = new();

    /// <summary>Órfãos que não couberam na lista detalhada.</summary>
    public int ItensOmitidos { get; set; }
}

/// <summary>Um arquivo órfão no relatório.</summary>
public class OrfaoItemDto
{
    public long ArquivoId { get; set; }
    public string Nome { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
    public DateTime CriadoEm { get; set; }
    public string? Autor { get; set; }
}
