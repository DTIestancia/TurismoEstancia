using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>DTO de categoria de ponto turístico.</summary>
public class CategoriaPontoTuristicoDto
{
    public int Id { get; set; }
    public string Chave { get; set; } = null!;
    public string Nome { get; set; } = null!;
    public string? SubTitulo { get; set; }
    public string? Cor { get; set; }
    public string? Icone { get; set; }
    public bool ApresentarEmMaravilhas { get; set; } = true;
    public bool ExibirNoMapa { get; set; } = true;
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
    public int QuantidadePontos { get; set; }
}
