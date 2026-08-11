using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Galeria.Interfaces;

/// <summary>Serviço da Galeria de Estância (categorias dinâmicas + fotos otimizadas).</summary>
public interface IGaleriaService
{
    // ---- Categorias ----
    Task<IReadOnlyList<GaleriaCategoriaDto>> ListarCategoriasAsync(bool incluirInativas = false, CancellationToken ct = default);

    Task<GaleriaCategoriaDto?> ObterCategoriaPorIdAsync(int id, CancellationToken ct = default);

    Task<GaleriaCategoriaDto?> ObterCategoriaPorChaveAsync(string chave, CancellationToken ct = default);

    /// <summary>Salva a categoria (cria ou atualiza); <paramref name="capa"/> substitui a capa atual quando enviada.</summary>
    Task SalvarCategoriaAsync(GaleriaCategoriaDto dto, IFormFile? capa = null, CancellationToken ct = default);

    Task ExcluirCategoriaAsync(int id, CancellationToken ct = default);

    // ---- Fotos ----
    Task<IReadOnlyList<GaleriaMidiaDto>> ListarFotosAsync(int categoriaId, bool apenasAtivos = true, CancellationToken ct = default);

    /// <summary>Fotos ativas de todas as categorias ativas (com nome/chave da categoria), em uma única consulta.</summary>
    Task<IReadOnlyList<GaleriaMidiaDto>> ListarFotosTodasAsync(bool apenasAtivos = true, CancellationToken ct = default);

    /// <summary>Otimiza (redimensiona + re-encoda + remove EXIF) cada foto e vincula à categoria.</summary>
    Task AdicionarFotosAsync(int categoriaId, IEnumerable<IFormFile> fotos, CancellationToken ct = default);

    /// <summary>Fotos (de outras categorias) ainda não vinculadas à categoria — para o seletor do painel.</summary>
    Task<IReadOnlyList<GaleriaMidiaDto>> ListarFotosDisponiveisAsync(int categoriaId, CancellationToken ct = default);

    /// <summary>Vincula fotos já existentes (pelos ArquivoId) à categoria, REUTILIZANDO os binários otimizados.</summary>
    Task VincularFotosAsync(int categoriaId, IEnumerable<long> arquivoIds, CancellationToken ct = default);

    /// <summary>Atualiza legenda e visibilidade da foto.</summary>
    Task AtualizarFotoAsync(GaleriaMidiaDto dto, CancellationToken ct = default);

    /// <summary>Move a foto uma posição para cima (-1) ou para baixo (+1) dentro da categoria.</summary>
    Task MoverFotoAsync(int id, int direcao, CancellationToken ct = default);

    /// <summary>Remove a foto e apaga os binários da tabela Arquivo (imagem + thumbnail).</summary>
    Task ExcluirFotoAsync(int id, CancellationToken ct = default);

    // ---- Engajamento (visualizações e curtidas) ----

    /// <summary>Incrementa o contador de visualizações da foto e retorna o novo total.</summary>
    Task<int> RegistrarVisualizacaoAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Registra uma curtida ("Amei") com dedup por sessão anônima: a mesma sessão
    /// só curte uma vez. Retorna o total atualizado e o estado da operação.
    /// </summary>
    Task<GaleriaCurtidaResultado> CurtirAsync(int id, string sessaoId, CancellationToken ct = default);
}
