using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Infra.Interfaces;

/// <summary>
/// Ferramenta de manutenção do acervo de imagens: aplica em toda foto já gravada
/// a mesma otimização do upload (ver <c>OtimizadorDeImagem</c>) e informa o
/// antes/depois em bytes.
/// </summary>
public interface IRecompressorImagensService
{
    /// <summary>
    /// Varre as fotos da tabela <c>Arquivo</c> e regrava as que ficam menores.
    /// É idempotente: rodar de novo não altera nada (foto já otimizada é pulada
    /// só lendo o cabeçalho) e nenhum arquivo aumenta de tamanho.
    /// </summary>
    /// <param name="simular">True para medir e relatar sem gravar nada.</param>
    /// <param name="progresso">Chamado a cada lote com o resultado parcial (visão de quem exibe).</param>
    Task<RecompressaoImagensDto> ExecutarAsync(
        bool simular = false,
        Action<RecompressaoImagensDto>? progresso = null,
        CancellationToken ct = default);
}
