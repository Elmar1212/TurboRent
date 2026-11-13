using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TurboRent.Modules.Catalog.Infrastructure.Persistence;

public sealed class CatalogDesignTimeFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? "Host=127.0.0.1;Database=turborent_catalog_design_only";
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(connection,
                postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "catalog"))
            .Options;
        return new CatalogDbContext(options);
    }
}
