using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TurismoEstancia.Identity.Models;

namespace TurismoEstancia.Identity.Data;

/// <summary>
/// Banco separado do ASP.NET Identity. Mantenha-o intocado:
/// não adicione migrações/seed de negócio aqui.
/// </summary>
public class IdentityContext : IdentityDbContext<Usuario>
{
    public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
