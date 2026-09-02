namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Item da seção "Conheça Estância" da home (foto em tela cheia + título +
/// descrição sobreposta). Conteúdo exclusivo, gerenciado no painel.
/// </summary>
public class ConhecaEstanciaItem
{
    public int Id { get; set; }

    /// <summary>Aba da seção em que o item aparece.</summary>
    public CategoriaConhecaEstancia Categoria { get; set; }

    public string Nome { get; set; } = null!;

    /// <summary>Texto sobreposto à foto na seção.</summary>
    public string? Descricao { get; set; }

    /// <summary>Foto de fundo (ocupa a seção inteira).</summary>
    public long? ImagemArquivoId { get; set; }

    public Arquivo? Imagem { get; set; }

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;
}
