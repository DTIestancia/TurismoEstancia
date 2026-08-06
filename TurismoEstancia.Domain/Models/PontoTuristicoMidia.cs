namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Imagem vinculada a um ponto turístico. Normaliza a mídia do ponto e
/// habilita galeria de fotos. Índice único filtrado garante no máximo 1 <see cref="TipoMidia.Capa"/> por ponto.
/// </summary>
public class PontoTuristicoMidia
{
    public int Id { get; set; }

    public int PontoTuristicoId { get; set; }

    public PontoTuristico? PontoTuristico { get; set; }

    public long ArquivoId { get; set; }

    public Arquivo? Arquivo { get; set; }

    public TipoMidia Tipo { get; set; }

    public int Ordem { get; set; }
}
