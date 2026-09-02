using System.ComponentModel.DataAnnotations;
using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de item da seção "Conheça Estância".</summary>
public class ConhecaEstanciaItemDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Selecione a aba (categoria).")]
    public CategoriaConhecaEstancia Categoria { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [MaxLength(200)]
    public string Nome { get; set; } = null!;

    [MaxLength(1000)]
    public string? Descricao { get; set; }

    /// <summary>Conteúdo completo da página de detalhe.</summary>
    public string? Corpo { get; set; }

    public long? ImagemArquivoId { get; set; }

    /// <summary>Zoom (%) do recorte da imagem (100–250).</summary>
    public int ImagemZoom { get; set; } = 100;

    /// <summary>Posição horizontal do foco (object-position X, 0–100).</summary>
    public int ImagemPosicaoX { get; set; } = 50;

    /// <summary>Posição vertical do foco (object-position Y, 0–100).</summary>
    public int ImagemPosicaoY { get; set; } = 50;

    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
