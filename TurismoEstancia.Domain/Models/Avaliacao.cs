namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Avaliação de visitante sobre um ponto turístico.
/// Entra como Aprovada = false (aguardando moderação no CMS).
/// </summary>
public class Avaliacao
{
    public int Id { get; set; }

    public int PontoTuristicoId { get; set; }

    public PontoTuristico? PontoTuristico { get; set; }

    public string Nome { get; set; } = null!;

    /// <summary>Nota de 1 a 5.</summary>
    public int Nota { get; set; }

    public string? Comentario { get; set; }

    public DateTime Data { get; set; }

    /// <summary>Só avaliações aprovadas aparecem no portal.</summary>
    public bool Aprovada { get; set; } = false;
}
