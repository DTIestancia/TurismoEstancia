using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Planeje.Interfaces;

/// <summary>Serviço do "Planeje sua viagem": categorias, cards e avaliações.</summary>
public interface IPlanejeService
{
    Task<IReadOnlyList<PlanejeCategoriaDto>> ListarCategoriasAsync(bool apenasAtivas = true, CancellationToken ct = default);
    Task<PlanejeCategoriaDto?> ObterCategoriaAsync(int id, CancellationToken ct = default);
    Task SalvarCategoriaAsync(PlanejeCategoriaDto dto, IFormFile? imagemPadrao, CancellationToken ct = default);
    Task ExcluirCategoriaAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<PlanejeItemDto>> ListarItensAsync(bool apenasAtivos = true, CancellationToken ct = default);
    Task<PlanejeItemDto?> ObterItemAsync(int id, CancellationToken ct = default);
    Task SalvarItemAsync(PlanejeItemDto dto, IFormFile? imagem, CancellationToken ct = default);
    Task ExcluirItemAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<PlanejeAvaliacaoDto>> ListarAvaliacoesAsync(int itemId, bool apenasAprovadas = true, CancellationToken ct = default);
    Task<IReadOnlyList<PlanejeAvaliacaoDto>> ListarPendentesAsync(CancellationToken ct = default);
    Task SubmeterAvaliacaoAsync(PlanejeAvaliacaoDto dto, CancellationToken ct = default);
    Task AprovarAvaliacaoAsync(int id, CancellationToken ct = default);
    Task ExcluirAvaliacaoAsync(int id, CancellationToken ct = default);
    Task<int> ContarPendentesAsync(CancellationToken ct = default);
}
