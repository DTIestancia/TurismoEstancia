using System.Buffers;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TurismoEstancia.Domain.Data;

namespace TurismoEstancia.Services.Infra.Arquivos;

/// <summary>
/// Fluxo somente-leitura sobre o binário guardado em <c>Arquivos.ArquBytes</c>,
/// buscado do banco em janelas.
///
/// É o que permite servir mídia <b>direto da tabela</b>: nada é gravado em disco
/// (nem no deploy, nem no perfil do usuário do serviço) e o arquivo nunca é
/// materializado inteiro em memória — um vídeo de 11 MB ocupa no máximo o tamanho
/// da janela (1 MB) por requisição em andamento, e o restante vai sendo buscado
/// conforme a leitura avança, em fluxo sequencial.
///
/// A janela é um buffer <b>alugado do pool</b> e reaproveitado entre as leituras do
/// mesmo fluxo: alocar um array novo por janela jogava 1 MB por vez no <i>large
/// object heap</i> (medido: 20 vídeos de 15 MB simultâneos subiam o processo de
/// 127 MB para 265 MB, quase tudo lixo ainda não coletado). Com o pool, o que fica
/// retido é uma janela por requisição, sem lixo.
///
/// O fluxo é pesquisável (<see cref="CanSeek"/>) de propósito: é assim que o
/// ASP.NET atende <c>Range</c> — o que faz a barra do player funcionar — e o
/// <c>HEAD</c>, que só precisa de <see cref="Length"/>.
/// </summary>
public sealed class FluxoDoArquivoNoBanco : Stream
{
    /// <summary>Bytes buscados por consulta: o teto de memória de uma leitura.</summary>
    private const int TamanhoDaJanela = 1024 * 1024;

    private readonly AppDbContext _db;
    private readonly long _id;
    private readonly long _tamanho;

    private byte[]? _janela;
    private int _valido;
    private long _inicioDaJanela = -1;
    private long _posicao;
    private bool _devolvida;

    public FluxoDoArquivoNoBanco(AppDbContext db, long id, long tamanho)
    {
        _db = db;
        _id = id;
        _tamanho = tamanho;
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => _tamanho;

    public override long Position
    {
        get => _posicao;
        set
        {
            if (value < 0 || value > _tamanho)
                throw new ArgumentOutOfRangeException(nameof(value));
            _posicao = value;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        GarantirJanela(_posicao);
        return Copiar(buffer.AsSpan(offset, count));
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await GarantirJanelaAsync(_posicao, cancellationToken);
        return Copiar(buffer.Span);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var destino = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _posicao + offset,
            SeekOrigin.End => _tamanho + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };

        if (destino < 0 || destino > _tamanho)
            throw new IOException("Posição fora do arquivo.");

        _posicao = destino;
        return _posicao;
    }

    public override void Flush()
    {
    }

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        // Nenhuma conexão fica aberta entre as janelas; a janela volta para o pool
        // (a memória é reaproveitada pelo próximo fluxo em vez de virar lixo).
        if (disposing && !_devolvida)
        {
            _devolvida = true;
            _valido = 0;
            _inicioDaJanela = -1;

            if (_janela is not null)
            {
                ArrayPool<byte>.Shared.Return(_janela);
                _janela = null;
            }
        }

        base.Dispose(disposing);
    }

    /// <summary>Copia da janela atual o que couber no destino e avança a posição (0 = fim).</summary>
    private int Copiar(Span<byte> destino)
    {
        if (_posicao >= _tamanho || _valido == 0 || destino.IsEmpty)
            return 0;

        var deslocamento = (int)(_posicao - _inicioDaJanela);
        if (deslocamento < 0 || deslocamento >= _valido)
            return 0;

        var total = Math.Min(_valido - deslocamento, destino.Length);
        _janela.AsSpan(deslocamento, total).CopyTo(destino);
        _posicao += total;
        return total;
    }

    private void GarantirJanela(long posicao)
    {
        if (posicao >= _tamanho || JanelaCobre(posicao))
            return;

        DefinirJanela(posicao, PreencherAsync(posicao, CancellationToken.None).GetAwaiter().GetResult());
    }

    private async ValueTask GarantirJanelaAsync(long posicao, CancellationToken ct)
    {
        if (posicao >= _tamanho || JanelaCobre(posicao))
            return;

        DefinirJanela(posicao, await PreencherAsync(posicao, ct));
    }

    private void DefinirJanela(long pedido, int validos)
    {
        _valido = validos;
        _inicioDaJanela = validos > 0 ? pedido : -1;
    }

    private bool JanelaCobre(long posicao) =>
        _inicioDaJanela >= 0 && posicao >= _inicioDaJanela && posicao < _inicioDaJanela + _valido;

    /// <summary>
    /// Preenche a janela com o bloco pedido. O <c>SUBSTRING</c> faz o servidor mandar
    /// só o trecho necessário (o custo na rede é o do trecho, não o do arquivo) e o
    /// <c>GetBytes</c> com <c>SequentialAccess</c> escreve direto no buffer alugado,
    /// sem o driver materializar a coluna em um array novo. Devolve quantos bytes
    /// ficaram válidos: a linha desaparecer no meio da leitura significa fim do fluxo.
    /// </summary>
    private async Task<int> PreencherAsync(long posicao, CancellationToken ct)
    {
        var tamanho = (int)Math.Min(TamanhoDaJanela, _tamanho - posicao);
        if (tamanho <= 0)
            return 0;

        var buffer = _janela ??= ArrayPool<byte>.Shared.Rent(TamanhoDaJanela);
        var conexao = _db.Database.GetDbConnection();
        var abriuAgora = conexao.State != ConnectionState.Open;
        if (abriuAgora)
            await conexao.OpenAsync(ct);

        try
        {
            using var comando = conexao.CreateCommand();
            // SUBSTRING é 1-based: o byte 0 do fluxo é o caractere 1.
            comando.CommandText =
                "SELECT SUBSTRING(ArquBytes, @inicio + 1, @tamanho) FROM Arquivos WHERE ArquId = @id";
            Adicionar(comando, "@id", _id);
            Adicionar(comando, "@inicio", posicao);
            Adicionar(comando, "@tamanho", tamanho);

            using var leitor = await comando.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
            if (!await leitor.ReadAsync(ct))
                return 0;

            var lidos = 0;
            while (lidos < tamanho)
            {
                var lote = leitor.GetBytes(0, lidos, buffer, lidos, tamanho - lidos);
                if (lote <= 0)
                    break;

                lidos += (int)lote;
            }

            return lidos;
        }
        finally
        {
            if (abriuAgora)
                await conexao.CloseAsync();
        }
    }

    private static void Adicionar(DbCommand comando, string nome, object valor)
    {
        var parametro = comando.CreateParameter();
        parametro.ParameterName = nome;
        parametro.Value = valor;
        comando.Parameters.Add(parametro);
    }
}
