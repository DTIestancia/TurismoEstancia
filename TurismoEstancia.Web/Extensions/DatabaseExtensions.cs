using Microsoft.EntityFrameworkCore;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.IdentityClass.Data;
using TurismoEstancia.Web.Infrastructure;

namespace TurismoEstancia.Web.Extensions;

/// <summary>
/// Registra todos os DbContext do sistema — um por connection string.
/// </summary>
public static class DatabaseExtensions
{
    public static void AddDatabase(this WebApplicationBuilder builder)
    {
        var appCs = builder.Configuration.GetConnectionString("TurismoEstancia")
            ?? throw new InvalidOperationException("Connection string 'TurismoEstancia' não configurada.");
        builder.Services.AddSingleton<AuditoriaSaveChangesInterceptor>();
        builder.Services.AddDbContext<AppDbContext>((services, options) =>
            options.UseSqlServer(appCs, o => o.UseCompatibilityLevel(120))
                   .AddInterceptors(services.GetRequiredService<AuditoriaSaveChangesInterceptor>()));

        var identityCs = builder.Configuration.GetConnectionString("TurismoEstanciaIdentity")
            ?? throw new InvalidOperationException("Connection string 'TurismoEstanciaIdentity' não configurada.");
        builder.Services.AddDbContext<IdentityContext>(options =>
            options.UseSqlServer(identityCs, o => o.UseCompatibilityLevel(120)));
    }
}
