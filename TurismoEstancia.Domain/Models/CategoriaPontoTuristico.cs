namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Categoria de ponto turístico. Alimenta a seção "7 Maravilhas"
/// (<see cref="ApresentarEmMaravilhas"/>) e o mapa interativo (<see cref="ExibirNoMapa"/>).
/// </summary>
public class CategoriaPontoTuristico
{
    public int Id { get; set; }

    /// <summary>
    /// Chave estável da categoria (ex.: "heritage", "nature", "hotel", "food", "service").
    /// Usada no mapa (classes CSS, filtros e legenda) e no agrupamento das Maravilhas.
    /// </summary>
    public string Chave { get; set; } = null!;

    public string Nome { get; set; } = null!;

    /// <summary>Subtítulo exibido no cabeçalho da categoria (ex.: "A história viva de Estância").</summary>
    public string? SubTitulo { get; set; }

    /// <summary>Cor em hex usada no mapa (ex.: "#E63946").</summary>
    public string? Cor { get; set; }

    /// <summary>Nome do ícone lucide exibido na categoria.</summary>
    public string? Icone { get; set; }

    /// <summary>Indica se a categoria aparece na seção "7 Maravilhas".</summary>
    public bool ApresentarEmMaravilhas { get; set; } = true;

    /// <summary>Indica se a categoria aparece no mapa interativo.</summary>
    public bool ExibirNoMapa { get; set; } = true;

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<PontoTuristico> PontosTuristicos { get; set; } = new List<PontoTuristico>();
}
