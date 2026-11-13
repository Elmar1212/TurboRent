using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TurboRent.Modules.Catalog.Infrastructure.Persistence;
using Amazon.Runtime;
using Amazon.S3;
using TurboRent.Modules.Catalog.Infrastructure.Storage;

namespace TurboRent.Modules.Catalog;

public static class CatalogModule
{
    public static IServiceCollection AddCatalog(this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<CatalogDbContext>(options =>
        {
            var connection = configuration.GetConnectionString("Database")
                ?? throw new InvalidOperationException("ConnectionStrings:Database must be configured.");
            if (environment.IsEnvironment("Testing") &&
                !new Npgsql.NpgsqlConnectionStringBuilder(connection).Database!.StartsWith(
                    "turborent_identity_test_", StringComparison.Ordinal))
                throw new InvalidOperationException("Testing requires an isolated catalog test database.");
            options.UseNpgsql(connection, postgres => postgres.MigrationsHistoryTable(
                "__EFMigrationsHistory", "catalog"));
        });
        services.AddSingleton<IAmazonS3>(_ =>
        {
            var endpoint = configuration["Storage:S3Endpoint"];
            var accessKey = configuration["Storage:AccessKey"];
            var secretKey = configuration["Storage:SecretKey"];
            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(accessKey) ||
                string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("Vehicle image storage configuration is incomplete.");
            return new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey),
                new AmazonS3Config
                {
                    ServiceURL = endpoint,
                    ForcePathStyle = true,
                    AuthenticationRegion = configuration["Storage:S3Region"] ?? "garage"
                });
        });
        services.AddSingleton<IVehicleImageStorage>(provider =>
        {
            var bucket = configuration["Storage:Bucket"];
            if (string.IsNullOrWhiteSpace(bucket))
                throw new InvalidOperationException("Storage:Bucket must be configured.");
            return new S3VehicleImageStorage(provider.GetRequiredService<IAmazonS3>(), bucket);
        });
        return services;
    }
}
