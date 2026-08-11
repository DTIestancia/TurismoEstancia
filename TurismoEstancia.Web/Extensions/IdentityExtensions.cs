using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using TurismoEstancia.Authorization.Services;
using TurismoEstancia.IdentityClass.Data;
using TurismoEstancia.IdentityClass.Models;

namespace TurismoEstancia.Web.Extensions;

/// <summary>
/// Configura Identity, cookies e autorização. As policies por claim
/// (Gerenciador/Operador) são registradas aqui via ClaimObrigatoria.
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

        // Handler de claims + policies por perfil (nunca roles literais).
        builder.Services.AddSingleton<IAuthorizationHandler, AppClaimHandler>();

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Perfis.Gerenciador, policy =>
                policy.AddRequirements(new ClaimObrigatoria(Perfis.Gerenciador)));

            options.AddPolicy(Perfis.Operador, policy =>
                policy.AddRequirements(new ClaimObrigatoria(Perfis.Operador)));
        });
    }
}
