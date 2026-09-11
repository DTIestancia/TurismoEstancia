namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Categoria da seção "Planeje sua viagem" (ex.: Onde ficar, Onde comer,
/// Serviços). Cada categoria tem imagem padrão usada nos cards sem foto.
/// </summary>
public class PlanejeCategoria
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string? Descricao { get; set; }

    /// <summary>Nome do ícone lucide exibido no filtro e no card.</summary>
    public string? Icone { get; set; }

    /// <summary>Cor de destaque da categoria (hex).</summary>
    public string? Cor { get; set; }

    /// <summary>Imagem padrão dos cards da categoria sem foto própria.</summary>
    public long? ImagemPadraoArquivoId { get; set; }

    public Arquivo? ImagemPadrao { get; set; }

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<PlanejeItem> Itens { get; set; } = new List<PlanejeItem>();
}
