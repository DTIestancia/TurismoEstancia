namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de notícia.</summary>
public class NoticiaDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = null!;
    public string? Resumo { get; set; }
    public string? Corpo { get; set; }
    public long? ImagemArquivoId { get; set; }
    public DateTime DataPublicacao { get; set; }
    public string Slug { get; set; } = null!;
    public bool Publicada { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
