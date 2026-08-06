namespace TurismoEstancia.Domain.Models;

/// <summary>Grupo cultural da seção gastronomia (Reisado, Cacumbi, Batucada, Samba de Coco...).</summary>
public class GrupoCultural
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;
}
