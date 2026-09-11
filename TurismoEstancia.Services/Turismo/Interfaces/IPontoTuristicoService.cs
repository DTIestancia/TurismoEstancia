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
    Task SalvarAsync(PontoTuristicoDto dto, IFormFile? capa, IFormFile? pictograma, IEnumerable<IFormFile> galeria, IFormFile? icone = null, CancellationToken ct = default);

    /// <summary>Oculta o ponto do portal (Ativo = false).</summary>
    Task OcultarAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Exclusão definitiva: remove o ponto, suas mídias/horários/avaliações
    /// (filhos próprios) e os arquivos órfãos. Bloqueada se houver vínculo
    /// com itens de roteiro.
    /// </summary>
    Task ExcluirAsync(int id, CancellationToken ct = default);

    Task ReativarAsync(int id, CancellationToken ct = default);

    /// <summary>Atualiza apenas a posição percentual do ponto no mapa.</summary>
    Task AtualizarPosicaoAsync(int id, int leftPercent, int topPercent, CancellationToken ct = default);
    Task AtualizarPosicaoMobileAsync(int id, int leftPercent, int topPercent, CancellationToken ct = default);
}
