namespace TurismoEstancia.Domain.DTOs;

/// <summary>Resultado de uma tentativa de curtir uma foto da galeria.</summary>
public class GaleriaCurtidaResultado
{
    /// <summary>Total de curtidas após a operação.</summary>
    public int Curtidas { get; set; }

    /// <summary>True quando a curtida foi registrada nesta chamada.</summary>
    public bool Curtiu { get; set; }

    /// <summary>True quando a sessão já tinha curtido (dedup anônimo).</summary>
    public bool JaCurtiu { get; set; }

    /// <summary>Legenda da foto (para o ranking de analytics).</summary>
    public string? Titulo { get; set; }
}
