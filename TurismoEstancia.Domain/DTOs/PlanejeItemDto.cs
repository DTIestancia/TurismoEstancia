using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de card do "Planeje sua viagem".</summary>
public class PlanejeItemDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione a categoria.")]
    public int CategoriaId { get; set; }

    public string? CategoriaNome { get; set; }
    public string? CategoriaIcone { get; set; }
    public string? CategoriaCor { get; set; }
    public long? CategoriaImagemPadraoArquivoId { get; set; }

    [Required(ErrorMessage = "Informe o título.")]
    [MaxLength(180)]
    public string Titulo { get; set; } = null!;

    [MaxLength(2000)]
    public string? Descricao { get; set; }

    [MaxLength(255)]
    public string? Localizacao { get; set; }

    [MaxLength(100)]
    public string? Contato { get; set; }

    [MaxLength(255)]
    public string? Site { get; set; }

    [MaxLength(255)]
    public string? Instagram { get; set; }

    public long? ImagemArquivoId { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
