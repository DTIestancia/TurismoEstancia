using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Services.Analytics.Services;

/// <summary>
/// Drena a fila de eventos de analytics e grava em lote no banco (o request
/// nunca espera). Constrói o próprio escopo, como todo hosted service.
/// </summary>
public class AnalyticsWriterService : BackgroundService
{
    private readonly Channel<AnalyticsEvento> _canal;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AnalyticsWriterService> _logger;

    public AnalyticsWriterService(
        Channel<AnalyticsEvento> canal,
        IServiceScopeFactory scopeFactory,
        ILogger<AnalyticsWriterService> logger)
    {
        _canal = canal;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Drena o que já está na fila (lote por rajada de requests).
                var lote = new List<AnalyticsEvento>();
                while (_canal.Reader.TryRead(out var evento))
                {
                    lote.Add(evento);
                    if (lote.Count >= 250) break;
                }

                if (lote.Count > 0)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.AnalyticsEventos.AddRange(lote);
                    await db.SaveChangesAsync(stoppingToken);
                }

                // Aguarda o próximo evento (ou o cancelamento do host).
                await _canal.Reader.WaitToReadAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao gravar eventos de analytics.");
                try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }
}
