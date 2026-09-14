using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using TurismoEstancia.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Compressão Brotli/Gzip do HTML/CSS/JS (mídias já são comprimidas e ficam de fora).
builder.Services.AddResponseCompression(opcoes =>
{
    opcoes.EnableForHttps = true;
    opcoes.Providers.Add<BrotliCompressionProvider>();
    opcoes.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(opcoes => opcoes.Level = CompressionLevel.Fastest);

builder.AddDatabase();
builder.AddIdentityConfig();
builder.AddBusinessServices();
builder.AddInfrastructure();

var app = builder.Build();

app.UseStandardPipeline();
app.MapAllRoutes();

// Regra permanente do projeto: seed e alterações no banco do Identity são proibidos.
await app.RunAsync();
