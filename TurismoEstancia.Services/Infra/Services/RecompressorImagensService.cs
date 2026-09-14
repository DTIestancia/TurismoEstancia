using System.Linq.Expressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Infra.Imagens;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Services.Infra.Services;

/// <summary>
/// Recompressão do acervo de imagens que já está no banco — aplica em cada foto a
/// mesma regra usada no upload (ver <see cref="OtimizadorDeImagem"/>) e relata o
/// antes/depois em bytes.
///
/// Propriedades que o comando garante:
/// <list type="bullet">
/// <item><b>idempotente</b> — foto já no formato final (JPEG, dentro do limite, sem
/// EXIF) é reconhecida só pelo cabeçalho: não decodifica nem regrava. A segunda
/// execução não muda nada;</item>
/// <item><b>nunca cresce</b> — se o resultado não for menor que o original, o
/// original fica;</item>
/// <item><b>não deixa o cache mentindo</b> — os bytes da tabela são imutáveis por
/// contrato (o endpoint serve com <c>immutable</c>), então o comando descarta as
/// miniaturas derivadas em disco, que passariam a mostrar a versão antiga.</item>
/// </list>
///
/// A gravação é feita com <c>ExecuteUpdate</c> (sem carregar entidade rastreada):
/// cada foto vira um UPDATE e a memória fica presa a uma imagem por vez.
/// </summary>
public class RecompressorImagensService : IRecompressorImagensService
{
    /// <summary>Ids por consulta — a página traz só ids; cada foto é lida e gravada sozinha.</summary>
    private const int TamanhoPagina = 50;

    /// <summary>Quantas fotos entram na lista de maiores ganhos do relatório.</summary>
    private const int GanhosNoRelatorio = 12;

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<RecompressorImagensService> _logger;

    public RecompressorImagensService(AppDbContext db, IWebHostEnvironment env, ILogger<RecompressorImagensService> logger)
    {
        _db = db;
        _env = env;
        _logger = logger;
    }

    /// <summary>Linhas que o comando considera: mesma lista de tipos que o upload otimiza.</summary>
    private static readonly Expression<Func<Arquivo, bool>> EhFotoDeclarada =
        a => OtimizadorDeImagem.TiposDeFoto.Contains(a.ContentType);

    public async Task<RecompressaoImagensDto> ExecutarAsync(
        bool simular = false,
        Action<RecompressaoImagensDto>? progresso = null,
        CancellationToken ct = default)
    {
        var resultado = new RecompressaoImagensDto { Simulado = simular };

        var total = await _db.Arquivos.AsNoTracking().CountAsync(EhFotoDeclarada, ct);
        resultado.ForaDoEscopo = await _db.Arquivos.AsNoTracking()
            .CountAsync(a => !OtimizadorDeImagem.TiposDeFoto.Contains(a.ContentType), ct);

        var ultimoId = 0L;
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var ids = await _db.Arquivos.AsNoTracking()
                .Where(EhFotoDeclarada)
                .Where(a => a.Id > ultimoId)
                .OrderBy(a => a.Id)
                .Select(a => a.Id)
                .Take(TamanhoPagina)
                .ToListAsync(ct);

            if (ids.Count == 0) break;
            ultimoId = ids[^1];

            foreach (var id in ids)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    await ProcessarAsync(id, simular, resultado, ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Uma foto problemática não pode derrubar o acervo inteiro.
                    _logger.LogWarning(ex, "Falha ao recomprimir o arquivo {ArquivoId}.", id);
                    resultado.ComFalha++;
                }
            }

            progresso?.Invoke(resultado);
        }

        if (!simular)
        {
            var (miniaturas, bytes) = CacheDeMiniaturas.Limpar(_env.ContentRootPath);
            resultado.MiniaturasRemovidas = miniaturas;
            resultado.MiniaturasBytesRemovidos = bytes;
        }

        progresso?.Invoke(resultado);
        return resultado;
    }

    private async Task ProcessarAsync(long id, bool simular, RecompressaoImagensDto resultado, CancellationToken ct)
    {
        var foto = await _db.Arquivos.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new { a.Bytes, a.ContentType, a.Nome })
            .FirstOrDefaultAsync(ct);

        if (foto is null || foto.Bytes.Length == 0)
            return;

        var antes = foto.Bytes.LongLength;
        resultado.Analisados++;

        // Já está no formato final: é o que torna o comando idempotente (só lê o
        // cabeçalho — não decodifica o pixel nem regrava nada).
        if (await OtimizadorDeImagem.JaEstaOtimizadaAsync(foto.Bytes, OtimizadorDeImagem.MaxDimensaoPadrao, ct))
        {
            resultado.JaOtimizados++;
            Contabilizar(resultado, antes, antes);
            return;
        }

        var otimizada = await OtimizadorDeImagem.OtimizarAsync(foto.Bytes, foto.ContentType, ct: ct);
        if (otimizada is null)
        {
            resultado.NaoDecodificados++;
            Contabilizar(resultado, antes, antes);
            return;
        }

        if (otimizada.Bytes.LongLength >= antes)
        {
            resultado.SemGanho++;
            Contabilizar(resultado, antes, antes);
            return;
        }

        resultado.Otimizados++;
        Contabilizar(resultado, antes, otimizada.Bytes.LongLength);
        RegistrarGanho(resultado, id, foto.Nome, antes, otimizada.Bytes.LongLength);

        if (simular)
            return;

        var nome = OtimizadorDeImagem.NomeComExtensao(foto.Nome, otimizada.Extensao);
        var bytes = otimizada.Bytes;
        var tamanho = bytes.LongLength;
        var tipo = otimizada.ContentType;

        await _db.Arquivos
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.Bytes, bytes)
                .SetProperty(a => a.Size, tamanho)
                .SetProperty(a => a.ContentType, tipo)
                .SetProperty(a => a.Nome, nome), ct);
    }

    private static void Contabilizar(RecompressaoImagensDto resultado, long antes, long depois)
    {
        resultado.BytesAntes += antes;
        resultado.BytesDepois += depois;
    }

    /// <summary>Mantém no relatório as fotos que mais economizaram bytes.</summary>
    private static void RegistrarGanho(RecompressaoImagensDto resultado, long id, string nome, long antes, long depois)
    {
        resultado.MaioresGanhos.Add(new RecompressaoItemDto
        {
            ArquivoId = id,
            Nome = nome,
            BytesAntes = antes,
            BytesDepois = depois
        });

        if (resultado.MaioresGanhos.Count > GanhosNoRelatorio * 3)
        {
            resultado.MaioresGanhos = resultado.MaioresGanhos
                .OrderByDescending(g => g.Economia)
                .Take(GanhosNoRelatorio)
                .ToList();
        }
    }
}
