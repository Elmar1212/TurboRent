using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TurboRent.Modules.Catalog.Domain;

namespace TurboRent.Modules.Catalog.Infrastructure.Persistence;

internal static class CatalogSeedData
{
    private const string ResourceName =
        "TurboRent.Modules.Catalog.Data.azerbaijan-market-catalog.json";

    private static readonly CatalogSeedDocument Seed = Load();
    private static readonly Dictionary<string, int> MakeIds =
        Seed.Makes.ToDictionary(make => make.Slug, make => make.Id, StringComparer.Ordinal);
    private static readonly VehicleModel[] Models = Seed.Models.Select(ToVehicleModel).ToArray();

    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<VehicleMake>().HasData(Seed.Makes);
        builder.Entity<VehicleModel>().HasData(Models);
    }

    private static CatalogSeedDocument Load()
    {
        using var stream = typeof(CatalogSeedData).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Catalog seed resource '{ResourceName}' was not found.");

        return JsonSerializer.Deserialize<CatalogSeedDocument>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidDataException($"Catalog seed resource '{ResourceName}' is empty or invalid.");
    }

    private static VehicleModel ToVehicleModel(CatalogModelSeed model)
    {
        if (!MakeIds.TryGetValue(model.MakeSlug, out var makeId))
            throw new InvalidDataException($"Catalog model '{model.Slug}' references unknown make '{model.MakeSlug}'.");

        return new VehicleModel
        {
            Id = model.Id,
            MakeId = makeId,
            Slug = model.Slug,
            NameAz = model.NameAz,
            NameEn = model.NameEn
        };
    }

    private sealed class CatalogSeedDocument
    {
        public VehicleMake[] Makes { get; init; } = [];
        public CatalogModelSeed[] Models { get; init; } = [];
    }

    private sealed class CatalogModelSeed
    {
        public int Id { get; init; }
        public string MakeSlug { get; init; } = "";
        public string Slug { get; init; } = "";
        public string NameAz { get; init; } = "";
        public string NameEn { get; init; } = "";
    }
}
