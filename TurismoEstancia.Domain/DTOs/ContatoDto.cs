using System.ComponentModel.DataAnnotations;
using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de contato do rodapé.</summary>
public class ContatoDto
{
    public int Id { get; set; }
    public TipoContato Tipo { get; set; }
    public string? Rotulo { get; set; }

    [Required(ErrorMessage = "Informe o valor (telefone, link ou endereço).")]
    [MaxLength(500)]
    public string Valor { get; set; } = null!;
    public string? Icone { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
