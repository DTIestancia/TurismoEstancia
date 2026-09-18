namespace TurismoEstancia.Domain.DTOs;

/// <summary>
/// Resultado da recompressão do acervo de imagens (comando de manutenção, ver
/// <c>IRecompressorImagensService</c>). Os totais somam só as fotos do escopo —
/// vídeos, PDFs e SVG ficam de fora e aparecem como <see cref="ForaDoEscopo"/>.
/// </summary>
public class RecompressaoImagensDto
{
    /// <summary>Fotos varridas (linhas de <c>Arquivo</c> com tipo de foto).</summary>
    public int Analisados { get; set; }

    /// <summary>Fotos que ficaram menores e foram regravadas.</summary>
    public int Otimizados { get; set; }

    /// <summary>Fotos já no formato final (JPEG, dentro do limite, sem EXIF).</summary>
    public int JaOtimizados { get; set; }

    /// <summary>Fotos em que o resultado não ficou menor — o original foi mantido.</summary>
    public int SemGanho { get; set; }

    /// <summary>Fotos que o otimizador não conseguiu decodificar.</summary>
    public int NaoDecodificados { get; set; }

    /// <summary>Arquivos que falharam no meio do processo (seguem no relatório).</summary>
    public int ComFalha { get; set; }

    /// <summary>Linhas de <c>Arquivo</c> fora do escopo (vídeo, PDF, SVG, GIF, favicon...).</summary>
    public int ForaDoEscopo { get; set; }

    /// <summary>Soma em bytes de todas as fotos analisadas, antes.</summary>
    public long BytesAntes { get; set; }

    /// <summary>Soma em bytes depois (a foto que não mudou entra com o mesmo tamanho).</summary>
    public long BytesDepois { get; set; }

    /// <summary>True quando foi uma simulação (nada foi gravado).</summary>
    public bool Simulado { get; set; }

    /// <summary>Fotos com maior economia, para o relatório.</summary>
    public List<RecompressaoItemDto> MaioresGanhos { get; set; } = new();
}

/// <summary>Uma foto recompimida: quanto pesava e quanto pesa.</summary>
public class RecompressaoItemDto
{
    public long ArquivoId { get; set; }
    public string Nome { get; set; } = "";
    public long BytesAntes { get; set; }
    public long BytesDepois { get; set; }

    /// <summary>Bytes economizados (nunca negativo).</summary>
    public long Economia => BytesAntes - BytesDepois;
}
