using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Infra.Interfaces;

/// <summary>
/// Auditoria do acervo de arquivos: quais registros da tabela Arquivo não são usados
/// por nenhuma parte do sistema (ver <c>IArquivoService.IdsReferenciadosAsync</c>).
///
/// Serve para duas coisas: mostrar quanto de espaço está sobrando e permitir a
/// limpeza (<c>--excluir</c>), sempre com a regra de referência completa — a mesma
/// que protege a exclusão feita pelo painel.
/// </summary>
public interface IAuditoriaDeArquivosService
{
    /// <summary>
    /// Levanta os órfãos. Com <paramref name="excluir"/>, remove cada um deles
    /// (a exclusão revalida as referências, então nada em uso é tocado).
    /// </summary>
    Task<OrfaosDeArquivoDto> ExecutarAsync(bool excluir = false, CancellationToken ct = default);
}
