using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TurismoEstancia.IdentityClass.Models;

namespace TurismoEstancia.IdentityClass.Data
{

    public class IdentityContext : IdentityDbContext<Usuario>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options)
            : base(options)
        {
        }
    }
}