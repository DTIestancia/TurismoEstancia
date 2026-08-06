namespace TurismoEstancia.Domain.Models;

/// <summary>Grupo cultural (Reisado, Cacumbi, Batucada, Samba de Coco...).</summary>
public class GrupoCultural
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    /// <summary>Descrição completa (página de detalhe do portal).</summary>
    public string? Descricao { get; set; }

    /// <summary>Imagem do grupo (página de detalhe).</summary>
    public long? ImagemArquivoId { get; set; }

    public Arquivo? Imagem { get; set; }

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;
}
