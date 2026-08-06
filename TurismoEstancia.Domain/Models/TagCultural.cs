namespace TurismoEstancia.Domain.Models;

/// <summary>Tag cultural da seção cultura (ex.: "🔥 Barco de Fogo", "🎺 Filarmônicas").</summary>
public class TagCultural
{
    public int Id { get; set; }

    /// <summary>Nome exibido, normalmente com emoji no início.</summary>
    public string Nome { get; set; } = null!;

    /// <summary>Descrição completa (página de detalhe do portal).</summary>
    public string? Descricao { get; set; }

    /// <summary>Imagem da tag (página de detalhe).</summary>
    public long? ImagemArquivoId { get; set; }

    public Arquivo? Imagem { get; set; }

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;
}
