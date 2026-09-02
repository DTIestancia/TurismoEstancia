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

    public long? ImagemArquivoId { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
