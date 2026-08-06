namespace TurismoEstancia.Domain.Models;

/// <summary>Estatística exibida em cards na seção história (ex.: "192 · anos").</summary>
public class Estatistica
{
    public int Id { get; set; }

    /// <summary>Valor como texto (ex.: "192", "68 mil").</summary>
    public string Valor { get; set; } = null!;

    /// <summary>Legenda do valor (ex.: "anos", "habitantes").</summary>
    public string? Legenda { get; set; }

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;
}
