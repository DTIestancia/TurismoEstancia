namespace TurismoEstancia.Domain.Models;

/// <summary>Prato típico exibido na seção gastronomia.</summary>
public class PratoTuristico
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;
}
