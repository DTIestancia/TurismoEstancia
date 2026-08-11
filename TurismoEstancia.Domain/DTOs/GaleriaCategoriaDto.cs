using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de categoria da Galeria de Estância.</summary>
public class GaleriaCategoriaDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [MaxLength(100)]
    public string Nome { get; set; } = null!;

    /// <summary>Chave/slug da URL pública; se vazia, é gerada a partir do nome.</summary>
    [MaxLength(100)]
    public string Chave { get; set; } = null!;

    [MaxLength(500)]
    public string? Descricao { get; set; }

    /// <summary>Imagem de capa da categoria (tabela Arquivo) — card da galeria + OG/SEO.</summary>
    public long? CapaArquivoId { get; set; }

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;

    /// <summary>Total de fotos ativas da categoria.</summary>
    public int QuantidadeFotos { get; set; }

    /// <summary>Fotos da categoria (preenchido nas telas de detalhe).</summary>
    public IReadOnlyList<GaleriaMidiaDto> Midias { get; set; } = Array.Empty<GaleriaMidiaDto>();
}
