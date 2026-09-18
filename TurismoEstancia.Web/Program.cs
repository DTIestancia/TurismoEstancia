using System.IO.Compression;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using TurismoEstancia.Web.Comandos;
using TurismoEstancia.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Antispam dos formulários públicos (10 envios/min por IP em cada endpoint).
builder.Services.AddRateLimiter(opcoes =>
{
    opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opcoes.AddPolicy("formularios", http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = 10,
            QueueLimit = 0
        }));
});

// Compressão Brotli/Gzip do HTML/CSS/JS (mídias já são comprimidas e ficam de fora).
builder.Services.AddResponseCompression(opcoes =>
{
    opcoes.EnableForHttps = true;
    opcoes.Providers.Add<BrotliCompressionProvider>();
    opcoes.Providers.Add<GzipCompressionProvider>();
});
// Optimal (e não Fastest): HTML/CSS/JS são o que passa por aqui (mídia fica fora por MIME) e
// a CPU extra é desprezível diante do ganho de bytes no primeiro acesso.
builder.Services.Configure<BrotliCompressionProviderOptions>(opcoes => opcoes.Level = CompressionLevel.Optimal);

// Comandos de manutenção do acervo — não sobem o servidor web:
//   dotnet run --project TurismoEstancia.Web -- recomprimir-imagens [--simular]
//   dotnet run --project TurismoEstancia.Web -- arquivos-orfaos [--excluir]
var comandoDeManutencao = RecomprimirImagens.EhComando(args) || ArquivosOrfaos.EhComando(args);
if (comandoDeManutencao)
{
    // A saída do comando é o relatório. O log de comandos do EF (Information pelo
    // Default de appsettings.json) enterraria o relatório no console de quem roda
    // a manutenção — avisos e erros continuam aparecendo.
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
}

builder.AddDatabase();
builder.AddIdentityConfig();
builder.AddBusinessServices();
builder.AddInfrastructure();

var app = builder.Build();

if (comandoDeManutencao)
{
    Environment.ExitCode = RecomprimirImagens.EhComando(args)
        ? await RecomprimirImagens.ExecutarAsync(app, args)
        : await ArquivosOrfaos.ExecutarAsync(app, args);
    return;
}

app.UseStandardPipeline();
app.MapAllRoutes();

// Regra permanente do projeto: seed e alterações no banco do Identity são proibidos.
await app.RunAsync();
