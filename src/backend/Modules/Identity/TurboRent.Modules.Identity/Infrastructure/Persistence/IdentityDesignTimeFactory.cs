using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TurboRent.Modules.Identity.Infrastructure.Persistence;

public sealed class IdentityDesignTimeFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        // Only model generation: this is not a live connection and contains no credentials.
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql("Host=127.0.0.1;Database=turborent_design_only",
                postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "identity"))
            .Options;
        return new IdentityDbContext(options);
    }
}
