using System.ComponentModel.DataAnnotations;
using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Domain.DTOs;

/// <summary>
/// DTO de ponto turístico com mídias e horários (usado no form do CMS
/// e na composição do portal).
/// </summary>
public class PontoTuristicoDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [MaxLength(150)]
    public string Nome { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Selecione a categoria.")]
    public int CategoriaId { get; set; }
    public string? Descricao { get; set; }
    public string? Detalhe { get; set; }
    public string? Tag { get; set; }
    public string? Icone { get; set; }
    public string? CategoriaNome { get; set; }
    public string? CategoriaCor { get; set; }
    public string? CategoriaIcone { get; set; }
    public bool CategoriaApresentarEmMaravilhas { get; set; }
    public string? Endereco { get; set; }
    public string? ComoChegar { get; set; }
    public int LeftPercent { get; set; }
    public int TopPercent { get; set; }
    public bool ExibirNoMapa { get; set; } = true;
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;

    // Mídias e horários (incluídos quando solicitado).
    public List<PontoTuristicoMidiaDto> Midias { get; set; } = new();
    public List<HorarioFuncionamentoDto> Horarios { get; set; } = new();

    // Atalhos da capa/pictograma para o portal.
    public long? CapaArquivoId => Midias.FirstOrDefault(m => m.Tipo == TipoMidia.Capa)?.ArquivoId;
    public long? PictogramaArquivoId => Midias.FirstOrDefault(m => m.Tipo == TipoMidia.Pictograma)?.ArquivoId;
}
