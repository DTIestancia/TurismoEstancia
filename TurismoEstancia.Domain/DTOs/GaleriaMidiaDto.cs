using System.ComponentModel.DataAnnotations;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de foto da Galeria de Estância.</summary>
public class GaleriaMidiaDto
{
    public int Id { get; set; }

    public int CategoriaId { get; set; }

    /// <summary>Nome da categoria (preenchido nas consultas do portal).</summary>
    public string? CategoriaNome { get; set; }

    /// <summary>Chave da categoria (link /galeria/{chave}).</summary>
    public string? CategoriaChave { get; set; }

    /// <summary>Id da imagem otimizada na tabela Arquivo (lightbox).</summary>
    public long ArquivoId { get; set; }

    /// <summary>Id do thumbnail na tabela Arquivo (grids).</summary>
    public long? ArquivoThumbId { get; set; }

    [MaxLength(200)]
    public string? Titulo { get; set; }

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;

    /// <summary>Total de visualizações da foto.</summary>
    public int Visualizacoes { get; set; }

    /// <summary>Total de curtidas ("Amei").</summary>
    public int Curtidas { get; set; }
}
