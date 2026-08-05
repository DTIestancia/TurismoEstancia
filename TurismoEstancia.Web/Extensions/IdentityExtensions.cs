using Microsoft.AspNetCore.Identity;
using TurismoEstancia.Identity.Data;
using TurismoEstancia.Identity.Models;

namespace TurismoEstancia.Web.Extensions;

/// <summary>
/// Configura Identity, cookies e autorização. As policies por claim
/// (Gerenciador/Operador) são adicionadas na Fase 3 (Authorization).
/// </summary>
public static class IdentityExtensions
{
    public static void AddIdentityConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
        {
            // Senha forte
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;

            // Lockout: 5 tentativas / 5 minutos
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

            // v1: contas criadas pelo Gerenciador (sem auto-registro público)
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<IdentityContext>()
        .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = ".TurismoEstancia.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = builder.Environment.IsProduction()
                ? CookieSecurePolicy.Always
                : CookieSecurePolicy.SameAsRequest;
            options.LoginPath = "/Identity/Account/Login";
            options.LogoutPath = "/Identity/Account/Logout";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

        builder.Services.AddAuthorization();
    }
}
