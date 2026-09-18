using Microsoft.Extensions.Logging;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Services.Infra.Services;

/// <summary>
/// Órfãos do acervo: registros da tabela Arquivo que ninguém referencia. A regra de
/// "quem referencia" é a do <c>IArquivoService</c> (todas as colunas de id + as
/// seções que citam o id como texto) — a mesma que impede uma exclusão no painel de
/// apagar uma imagem em uso. Aqui ela é aplicada do outro lado: uma consulta por
/// fonte, e não uma por arquivo, para o levantamento caber em qualquer acervo.
/// </summary>
public class AuditoriaDeArquivosService : IAuditoriaDeArquivosService
{
    /// <summary>Quantos órfãos o relatório detalha (ordenados do maior para o menor).</summary>
    private const int ItensNoRelatorio = 25;

    private readonly IArquivoService _arquivos;
    private readonly ILogger<AuditoriaDeArquivosService> _logger;

    public AuditoriaDeArquivosService(IArquivoService arquivos, ILogger<AuditoriaDeArquivosService> logger)
    {
        _arquivos = arquivos;
        _logger = logger;
    }

    public async Task<OrfaosDeArquivoDto> ExecutarAsync(bool excluir = false, CancellationToken ct = default)
    {
        // Nenhuma das duas consultas traz bytes: o acervo é listado por metadados e as
        // referências vêm em uma passada por fonte.
        var acervo = await _arquivos.ListarResumoAsync(ct);
        var referenciados = await _arquivos.IdsReferenciadosAsync(ct);

        var orfaos = acervo.Where(a => !referenciados.Contains(a.Id)).ToList();

        var resultado = new OrfaosDeArquivoDto
        {
            Total = acervo.Count,
            Referenciados = acervo.Count - orfaos.Count,
            BytesTotal = acervo.Sum(a => a.Size),
            BytesOrfaos = orfaos.Sum(a => a.Size),
            Excluido = excluir
        };

        foreach (var orfao in orfaos.OrderByDescending(o => o.Size).Take(ItensNoRelatorio))
        {
            resultado.Itens.Add(new OrfaoItemDto
            {
                ArquivoId = orfao.Id,
                Nome = orfao.Nome,
                ContentType = orfao.ContentType,
                Size = orfao.Size,
                CriadoEm = orfao.CriadoEm,
                Autor = orfao.Autor
            });
        }

        resultado.ItensOmitidos = Math.Max(0, orfaos.Count - resultado.Itens.Count);

        if (!excluir)
            return resultado;

        foreach (var orfao in orfaos)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                // ExcluirAsync revalida as referências: se algo passou a usar o arquivo
                // entre o levantamento e agora, ele não é tocado.
                await _arquivos.ExcluirAsync(orfao.Id, ct);

                // Não sobra nada em disco para apagar junto: as versões reduzidas de
                // ?largura=N vivem no cache de memória de cada processo do portal (ver
                // ArquivoController) e o comando de manutenção roda com o portal
                // parado — que é o que descarta esse cache.
                resultado.Excluidos++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Falha ao excluir o arquivo órfão {ArquivoId}.", orfao.Id);
                resultado.ComFalha++;
            }
        }

        return resultado;
    }
}
