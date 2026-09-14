namespace TurismoEstancia.Web.Models;

/// <summary>Ajuste de recorte (zoom + posição) da capa de uma página interna.</summary>
public class AjusteCapaViewModel
{
    public int Zoom { get; set; } = 100;
    public int PosX { get; set; } = 50;
    public int PosY { get; set; } = 50;

    /// <summary>URL da imagem atual para a prévia (null quando ainda não há).</summary>
    public string? Src { get; set; }
}
