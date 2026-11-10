using System.Diagnostics;
using System.Security.Claims;
using TurismoEstancia.Authorization.Services;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Web.Middleware;

public sealed class AuditoriaMiddleware
{
    private readonly RequestDelegate _next;

    public AuditoriaMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (!EhRotaAuditavel(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var inicio = Stopwatch.GetTimestamp();
        try
        {
            await _next(context);
        }
        finally
        {
            var tipo = Classificar(context);
            db.Auditorias.Add(new AuditoriaAcao
            {
                DataHoraUtc = DateTime.UtcNow,
                Tipo = tipo,
                Area = ObterArea(context.Request.Path),
                UsuarioId = context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                UsuarioNome = ObterUsuarioNome(context),
                Perfis = string.Join(",", context.User.FindAll(Perfis.TipoClaim).Select(c => c.Value).Distinct()),
                Rota = Limitar(context.Request.Path.Value, 500),
                MetodoHttp = Limitar(context.Request.Method, 10),
                StatusHttp = context.Response.StatusCode,
                DuracaoMs = (long)(Stopwatch.GetElapsedTime(inicio).TotalMilliseconds),
                EnderecoIp = Limitar(context.Connection.RemoteIpAddress?.ToString(), 64),
                Navegador = Limitar(context.Request.Headers.UserAgent.ToString(), 500),
                Correlacao = Limitar(context.TraceIdentifier, 100),
                Sucesso = context.Response.StatusCode is >= 200 and < 400,
                Mensagem = context.Response.StatusCode >= 400 ? "Resposta administrativa com falha ou acesso negado." : null
            });

            try { await db.SaveChangesAsync(); }
            catch { /* a auditoria nunca deve derrubar a resposta principal */ }
        }
    }

    private static bool EhRotaAuditavel(PathString rota) =>
        rota.StartsWithSegments("/Gerenciador") ||
        rota.StartsWithSegments("/Operador") ||
        rota.StartsWithSegments("/Identity/Account/Login") ||
        rota.StartsWithSegments("/Identity/Account/Logout") ||
        rota.StartsWithSegments("/Identity/Account/AccessDenied");

    private static string Classificar(HttpContext context)
    {
        var rota = context.Request.Path.Value ?? "";
        if (rota.StartsWith("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase))
            return context.Request.Method == HttpMethods.Post
                ? context.Response.StatusCode is >= 300 and < 400 ? "LoginSucesso" : "LoginFalha"
                : "AcessoTela";
        if (rota.StartsWith("/Identity/Account/Logout", StringComparison.OrdinalIgnoreCase)) return "Logout";
        if (rota.StartsWith("/Identity/Account/AccessDenied", StringComparison.OrdinalIgnoreCase)) return "AcessoNegado";
        if (context.Response.StatusCode is 401 or 403) return "AcessoNegado";
        return EhEntrada(context.Request.Path) ? "EntradaPainel" : "AcessoTela";
    }

    private static bool EhEntrada(PathString rota) =>
        rota.Equals("/Gerenciador", StringComparison.OrdinalIgnoreCase) ||
        rota.Equals("/Operador", StringComparison.OrdinalIgnoreCase) ||
        rota.StartsWithSegments("/Gerenciador/Dashboard") ||
        rota.StartsWithSegments("/Operador/Dashboard");

    private static string? ObterUsuarioNome(HttpContext context)
    {
        var nome = context.User.Identity?.Name ?? context.User.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrWhiteSpace(nome)) return nome;
        if (context.Request.Method == HttpMethods.Post &&
            context.Request.Path.StartsWithSegments("/Identity/Account/Login") &&
            context.Request.HasFormContentType)
            return context.Request.Form["Email"].ToString();
        return null;
    }

    private static string ObterArea(PathString rota)
    {
        if (rota.StartsWithSegments("/Gerenciador")) return "Gerenciador";
        if (rota.StartsWithSegments("/Operador")) return "Operador";
        return "Identity";
    }

    private static string? Limitar(string? valor, int limite) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Length <= limite ? valor : valor[..limite];
}
