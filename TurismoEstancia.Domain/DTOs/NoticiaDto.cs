using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de notícia.</summary>
public class NoticiaDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o título.")]
    [MaxLength(180)]
    public string Titulo { get; set; } = null!;
    public string? Resumo { get; set; }
    public string? Corpo { get; set; }
    public long? ImagemArquivoId { get; set; }

    /// <summary>Galeria (categoria da Galeria de Estância) relacionada — opcional.</summary>
    public int? GaleriaCategoriaId { get; set; }

    /// <summary>Nome da galeria relacionada (exibição no painel e no portal).</summary>
    public string? GaleriaNome { get; set; }

    public DateTime DataPublicacao { get; set; }

    /// <summary>Slug da URL amigável; vazio no cadastro (gerado a partir do título).</summary>
    public string? Slug { get; set; }

    public bool Publicada { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
