using System.Threading.Channels;

namespace TurismoEstancia.Mail;

/// <summary>
/// Fila em memória (Channel) de e-mails a enviar — padrão do blueprint:
/// trabalhos lentos nunca são aguardados no controller; são enfileirados
/// aqui e processados em background pelo EmailBackgroundService.
/// </summary>
public interface IEmailQueue
{
    /// <summary>Enfileira um e-mail. Lança se a fila estiver cheia.</summary>
    void Enqueue(EmailJob job);

    /// <summary>Quantidade de e-mails aguardando envio.</summary>
    int Count { get; }

    /// <summary>Leitor consumido pelo worker.</summary>
    ChannelReader<EmailJob> Reader { get; }
}

/// <summary>Fila em memória (Channel) de e-mails — singleton.</summary>
public sealed class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailJob> _channel = Channel.CreateBounded<EmailJob>(
        new BoundedChannelOptions(5000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true
        });

    public int Count => _channel.Reader.Count;
    public ChannelReader<EmailJob> Reader => _channel.Reader;

    public void Enqueue(EmailJob job)
    {
        if (!_channel.Writer.TryWrite(job))
            throw new InvalidOperationException("Fila de e-mails cheia. Tente novamente em instantes.");
    }
}
