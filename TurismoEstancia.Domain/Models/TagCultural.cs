namespace TurismoEstancia.Domain.Models;

/// <summary>Tag cultural da seção cultura (ex.: "🔥 Barco de Fogo", "🎺 Filarmônicas").</summary>
public class TagCultural
{
    public int Id { get; set; }

    /// <summary>Nome exibido, normalmente com emoji no início.</summary>
    public string Nome { get; set; } = null!;

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;
}
