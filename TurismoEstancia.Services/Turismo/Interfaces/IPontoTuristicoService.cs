using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Turismo.Interfaces;

/// <summary>
/// Serviço de ponto turístico. Mídias (capa/pictograma/galeria) e horários
/// de funcionamento são tratados no mesmo formulário do CMS.
/// </summary>
public interface IPontoTuristicoService
{
    /// <summary>Lista pontos ativos ordenados, com dados da categoria e mídias (portal).</summary>
    Task<IReadOnlyList<PontoTuristicoDto>> ListarAsync(bool apenasAtivos = true, CancellationToken ct = default);

    /// <summary>Lista os POIs do mapa (com posição percentual e categoria).</summary>
    Task<IReadOnlyList<PontoTuristicoDto>> ListarParaMapaAsync(CancellationToken ct = default);

    Task<PontoTuristicoDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Salva o ponto, suas mídias (arquivos opcionais) e horários.</summary>
    Task SalvarAsync(PontoTuristicoDto dto, IFormFile? capa, IFormFile? pictograma, IEnumerable<IFormFile> galeria, CancellationToken ct = default);

    /// <summary>Exclusão lógica (Ativo = false).</summary>
    Task ExcluirAsync(int id, CancellationToken ct = default);

    Task ReativarAsync(int id, CancellationToken ct = default);

    /// <summary>Atualiza apenas a posição percentual do ponto no mapa.</summary>
    Task AtualizarPosicaoAsync(int id, int leftPercent, int topPercent, CancellationToken ct = default);
}
