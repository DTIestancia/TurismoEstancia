using TurismoEstancia.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddDatabase();
builder.AddIdentityConfig();
builder.AddBusinessServices();
builder.AddInfrastructure();

var app = builder.Build();

app.UseStandardPipeline();
app.MapAllRoutes();

// Regra permanente do projeto: seed e alterações no banco do Identity são proibidos.
await app.RunAsync();
