using Microsoft.EntityFrameworkCore;

namespace TurismoEstancia.Domain.Data;

/// <summary>
/// Contexto de dados do domínio (bounded context principal).
/// DbSets e configuração fluente são adicionados na Fase 2.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
