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

    /// <summary>Conteúdo completo exibido na página de detalhe (estilo blog).</summary>
    public string? Corpo { get; set; }

    /// <summary>Foto de fundo (ocupa a seção inteira).</summary>
    public long? ImagemArquivoId { get; set; }

    public Arquivo? Imagem { get; set; }

    /// <summary>Zoom (%) aplicado ao recorte da imagem (100 = sem zoom, até 250).</summary>
    public int ImagemZoom { get; set; } = 100;

    /// <summary>Posição horizontal do foco do recorte (object-position X, 0–100).</summary>
    public int ImagemPosicaoX { get; set; } = 50;

    /// <summary>Posição vertical do foco do recorte (object-position Y, 0–100).</summary>
    public int ImagemPosicaoY { get; set; } = 50;

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;
}
