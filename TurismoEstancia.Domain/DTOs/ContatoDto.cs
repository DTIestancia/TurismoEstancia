using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de contato do rodapé.</summary>
public class ContatoDto
{
    public int Id { get; set; }
    public TipoContato Tipo { get; set; }
    public string? Rotulo { get; set; }
    public string Valor { get; set; } = null!;
    public string? Icone { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}
