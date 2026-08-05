using TurismoEstancia.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddDatabase();
builder.AddIdentityConfig();
builder.AddBusinessServices();
builder.AddInfrastructure();

var app = builder.Build();

app.UseStandardPipeline();
app.MapAllRoutes();

await app.RunWithSeedSupportAsync(args);
