namespace TurismoEstancia.Domain.DTOs;

/// <summary>Resultado da busca pública do portal, agrupado por seção.</summary>
public class BuscaResultadoDto
{
    public string Termo { get; set; } = string.Empty;
    public IReadOnlyList<GrupoBuscaDto> Grupos { get; set; } = Array.Empty<GrupoBuscaDto>();
    public int Total { get; set; }
}

/// <summary>Um grupo de resultados (ex.: Maravilhas, Notícias).</summary>
public class GrupoBuscaDto
{
    public required string Titulo { get; init; }
    public required string Icone { get; init; }
    public IReadOnlyList<ItemBuscaDto> Itens { get; init; } = Array.Empty<ItemBuscaDto>();
}

/// <summary>Um resultado: título + resumo + link (+ foto opcional).</summary>
public class ItemBuscaDto
{
    public required string Titulo { get; init; }
    public string? Resumo { get; init; }
    public required string Url { get; init; }
    public string? Rotulo { get; init; }
    public long? ImagemArquivoId { get; init; }
}
