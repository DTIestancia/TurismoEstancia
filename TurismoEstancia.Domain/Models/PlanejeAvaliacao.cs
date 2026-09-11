namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Avaliação de visitante sobre um item do "Planeje sua viagem".
/// Nome é opcional; comentário exige ao menos 50 caracteres.
/// Entra como Aprovada = false (aguardando moderação no CMS).
/// </summary>
public class PlanejeAvaliacao
{
    public int Id { get; set; }

    public int PlanejeItemId { get; set; }

    public PlanejeItem? PlanejeItem { get; set; }

    public string? Nome { get; set; }

    /// <summary>Nota de 1 a 5.</summary>
    public int Nota { get; set; }

    public string Comentario { get; set; } = null!;

    public DateTime Data { get; set; }

    /// <summary>Só avaliações aprovadas aparecem no portal.</summary>
    public bool Aprovada { get; set; } = false;
}
