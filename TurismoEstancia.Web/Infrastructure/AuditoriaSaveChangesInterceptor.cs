using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TurismoEstancia.Authorization.Services;
using TurismoEstancia.Domain.Models;

namespace TurismoEstancia.Web.Infrastructure;

public sealed class AuditoriaSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> CamposIgnorados = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash", "PasswordConfirm", "SecurityStamp", "ConcurrencyStamp",
        "Token", "AccessToken", "RefreshToken", "Bytes", "FotoPerfil"
    };

    private readonly IHttpContextAccessor _http;

    public AuditoriaSaveChangesInterceptor(IHttpContextAccessor http) => _http = http;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        RegistrarAlteracoes(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        RegistrarAlteracoes(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void RegistrarAlteracoes(DbContext? contexto)
    {
        if (contexto is null) return;

        var http = _http.HttpContext;
        foreach (var entrada in contexto.ChangeTracker.Entries().ToArray())
        {
            if (entrada.Entity is AuditoriaAcao || entrada.State is EntityState.Unchanged or EntityState.Detached)
                continue;

            var tipo = entrada.State switch
            {
                EntityState.Added => "Criacao",
                EntityState.Modified => "Alteracao",
                EntityState.Deleted => "Exclusao",
                _ => null
            };
            if (tipo is null) continue;

            var auditoria = new AuditoriaAcao
            {
                DataHoraUtc = DateTime.UtcNow,
                Tipo = tipo,
                Area = ObterArea(http),
                UsuarioId = http?.User.FindFirstValue(ClaimTypes.NameIdentifier),
                UsuarioNome = http?.User.Identity?.Name ?? http?.User.FindFirstValue(ClaimTypes.Email),
                Perfis = string.Join(",", http?.User.FindAll(Perfis.TipoClaim).Select(c => c.Value).Distinct() ?? []),
                Rota = Limitar(http?.Request.Path.Value, 500),
                MetodoHttp = Limitar(http?.Request.Method, 10),
                EnderecoIp = Limitar(http?.Connection.RemoteIpAddress?.ToString(), 64),
                Navegador = Limitar(http?.Request.Headers.UserAgent.ToString(), 500),
                Correlacao = Limitar(http?.TraceIdentifier, 100),
                Entidade = Limitar(entrada.Metadata.ClrType.Name, 200),
                EntidadeId = ObterChave(entrada),
                ValoresAnterioresJson = Serializar(entrada, false),
                ValoresNovosJson = Serializar(entrada, true),
                Sucesso = true
            };

            contexto.Set<AuditoriaAcao>().Add(auditoria);
        }
    }

    private static string ObterArea(HttpContext? http)
    {
        var rota = http?.Request.Path.Value ?? "";
        if (rota.StartsWith("/Gerenciador", StringComparison.OrdinalIgnoreCase)) return "Gerenciador";
        if (rota.StartsWith("/Operador", StringComparison.OrdinalIgnoreCase)) return "Operador";
        if (rota.StartsWith("/Identity", StringComparison.OrdinalIgnoreCase)) return "Identity";
        return "Sistema";
    }

    private static string? ObterChave(EntityEntry entrada)
    {
        var valores = entrada.Properties
            .Where(p => p.Metadata.IsPrimaryKey())
            .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue?.ToString());
        return valores.Count == 0 ? null : JsonSerializer.Serialize(valores);
    }

    private static string? Serializar(EntityEntry entrada, bool atual)
    {
        if (entrada.State == EntityState.Added && !atual) return null;
        if (entrada.State == EntityState.Deleted && atual) return null;

        var valores = entrada.Properties
            .Where(p => !CamposIgnorados.Contains(p.Metadata.Name))
            .Where(p => entrada.State != EntityState.Modified || p.IsModified)
            .Select(p => new { p.Metadata.Name, Valor = Sanitizar(p.CurrentValue, atual ? p.CurrentValue : p.OriginalValue) })
            .ToDictionary(p => p.Name, p => p.Valor);
        return valores.Count == 0 ? null : JsonSerializer.Serialize(valores);
    }

    private static object? Sanitizar(object? valor, object? fallback)
    {
        var escolhido = valor ?? fallback;
        if (escolhido is byte[]) return "[omitido]";
        if (escolhido is string texto && texto.Length > 1000) return texto[..1000] + "…";
        return escolhido;
    }

    private static string? Limitar(string? valor, int limite) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Length <= limite ? valor : valor[..limite];
}
