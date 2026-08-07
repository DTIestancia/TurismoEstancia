using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TurismoEstancia.Mail;

/// <summary>
/// Worker que consome a fila de e-mails e envia cada job num escopo próprio
/// (padrão do blueprint: fila + BackgroundService). Falha de um destinatário
/// não derruba os demais — apenas registra no log e segue.
/// </summary>
public class EmailBackgroundService : BackgroundService
{
    private readonly IEmailQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(
        IEmailQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
            try
            {
                await sender.EnviarAsync(job.Para, job.Assunto, job.CorpoHtml, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao enviar e-mail para {Destinatario} ({Assunto}).",
                    job.Para, job.Assunto);
            }
        }
    }
}
