namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Ponto turístico (as 7 Maravilhas + POIs do mapa: hotéis, restaurantes, serviços).
/// As imagens ficam em <see cref="PontoTuristicoMidia"/> (capa, pictograma, galeria).
/// </summary>
public class PontoTuristico
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string? Descricao { get; set; }

    /// <summary>Texto detalhado exibido no modal.</summary>
    public string? Detalhe { get; set; }

    /// <summary>Rótulo curto exibido no card (ex.: "🎺 Música").</summary>
    public string? Tag { get; set; }

    /// <summary>Nome do ícone lucide exibido no card.</summary>
    public string? Icone { get; set; }

    public int CategoriaId { get; set; }

    public CategoriaPontoTuristico? Categoria { get; set; }

    public string? Endereco { get; set; }

    /// <summary>Instruções de como chegar exibidas no modal.</summary>
    public string? ComoChegar { get; set; }

    /// <summary>Posição horizontal (0–100) no mapa ilustrado.</summary>
    public int LeftPercent { get; set; }

    /// <summary>Posição vertical (0–100) no mapa ilustrado.</summary>
    public int TopPercent { get; set; }

    /// <summary>Indica se o ponto aparece como POI no mapa.</summary>
    public bool ExibirNoMapa { get; set; } = true;

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<PontoTuristicoMidia> Midias { get; set; } = new List<PontoTuristicoMidia>();

    public ICollection<HorarioFuncionamento> Horarios { get; set; } = new List<HorarioFuncionamento>();

    public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
}
