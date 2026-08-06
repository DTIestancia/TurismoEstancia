namespace TurismoEstancia.Domain.Models;

/// <summary>Roteiro turístico curado (ex.: "Roteiro 1 — Praia do Saco e Catedral").</summary>
public class Roteiro
{
    public int Id { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descricao { get; set; }

    public long? ImagemArquivoId { get; set; }

    public Arquivo? Imagem { get; set; }

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<RoteiroItem> Itens { get; set; } = new List<RoteiroItem>();
}
