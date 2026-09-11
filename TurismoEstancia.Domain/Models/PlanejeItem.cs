namespace TurismoEstancia.Domain.Models;

/// <summary>
/// Card da seção "Planeje sua viagem" (hotel, restaurante, guia local...).
/// Abre em modal com detalhes, contatos e avaliações.
/// </summary>
public class PlanejeItem
{
    public int Id { get; set; }

    public int CategoriaId { get; set; }

    public PlanejeCategoria? Categoria { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descricao { get; set; }

    public string? Localizacao { get; set; }

    public string? Contato { get; set; }

    public string? Site { get; set; }

    public string? Instagram { get; set; }

    public long? ImagemArquivoId { get; set; }

    public Arquivo? Imagem { get; set; }

    public int Ordem { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<PlanejeAvaliacao> Avaliacoes { get; set; } = new List<PlanejeAvaliacao>();
}
