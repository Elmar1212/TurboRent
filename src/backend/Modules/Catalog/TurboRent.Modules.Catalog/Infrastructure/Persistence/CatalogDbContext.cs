using Microsoft.EntityFrameworkCore;
using TurboRent.Modules.Catalog.Domain;

namespace TurboRent.Modules.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<VehicleMake> Makes => Set<VehicleMake>();
    public DbSet<VehicleModel> Models => Set<VehicleModel>();
    public DbSet<VehicleListing> Listings => Set<VehicleListing>();
    public DbSet<VehicleListingImage> ListingImages => Set<VehicleListingImage>();
    public DbSet<ListingReviewEvent> ListingReviewEvents => Set<ListingReviewEvent>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("catalog");
        builder.Entity<VehicleMake>(entity =>
        {
            entity.ToTable("vehicle_makes");
            entity.HasKey(make => make.Id);
            entity.HasIndex(make => make.Slug).IsUnique();
            entity.Property(make => make.Slug).HasMaxLength(40).IsRequired();
            entity.Property(make => make.NameAz).HasMaxLength(80).IsRequired();
            entity.Property(make => make.NameEn).HasMaxLength(80).IsRequired();
        });
        builder.Entity<VehicleModel>(entity =>
        {
            entity.ToTable("vehicle_models");
            entity.HasKey(model => model.Id);
            entity.HasIndex(model => new { model.MakeId, model.Slug }).IsUnique();
            entity.Property(model => model.Slug).HasMaxLength(40).IsRequired();
            entity.Property(model => model.NameAz).HasMaxLength(80).IsRequired();
            entity.Property(model => model.NameEn).HasMaxLength(80).IsRequired();
            entity.HasOne<VehicleMake>().WithMany(make => make.Models)
                .HasForeignKey(model => model.MakeId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<VehicleListing>(entity =>
        {
            entity.ToTable("vehicle_listings", table =>
            {
                table.HasCheckConstraint("ck_listing_year", "\"Year\" BETWEEN 1980 AND 2035");
                table.HasCheckConstraint("ck_listing_price", "\"DailyPrice\" > 0 AND \"DailyPrice\" <= 99999");
                table.HasCheckConstraint("ck_listing_city", "\"City\" IN ('baku','ganja','sumgait','lankaran','shaki','gabala')");
                table.HasCheckConstraint("ck_listing_transmission", "\"Transmission\" IN ('automatic','manual')");
                table.HasCheckConstraint("ck_listing_fuel", "\"FuelType\" IN ('petrol','diesel','hybrid','electric')");
                table.HasCheckConstraint("ck_listing_seats", "\"Seats\" BETWEEN 2 AND 9");
                table.HasCheckConstraint("ck_listing_status", "\"Status\" IN ('Draft','Pending','Approved','Rejected','Suspended','Archived')");
            });
            entity.HasKey(listing => listing.Id);
            entity.HasIndex(listing => new { listing.Status, listing.City, listing.DailyPrice });
            entity.HasIndex(listing => new { listing.Status, listing.ModelId, listing.Year });
            entity.HasIndex(listing => listing.OwnerUserId);
            entity.HasIndex(listing => new { listing.Status, listing.CreatedAtUtc });
            entity.Property(listing => listing.Status).IsConcurrencyToken();
            entity.Property(listing => listing.City).HasMaxLength(20).IsRequired();
            entity.Property(listing => listing.DailyPrice).HasPrecision(10, 2);
            entity.Property(listing => listing.Transmission).HasMaxLength(16).IsRequired();
            entity.Property(listing => listing.FuelType).HasMaxLength(16).IsRequired();
            entity.Property(listing => listing.DescriptionAz).HasMaxLength(2000).IsRequired();
            entity.Property(listing => listing.DescriptionEn).HasMaxLength(2000).IsRequired();
            entity.Property(listing => listing.Status).HasMaxLength(16).IsRequired();
            entity.HasOne(listing => listing.Model).WithMany().HasForeignKey(listing => listing.ModelId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(listing => listing.Images).WithOne().HasForeignKey(image => image.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<VehicleListingImage>(entity =>
        {
            entity.ToTable("vehicle_listing_images", table =>
            {
                table.HasCheckConstraint("ck_listing_image_type",
                    "\"ContentType\" IN ('image/jpeg','image/png','image/webp')");
                table.HasCheckConstraint("ck_listing_image_length", "\"Length\" BETWEEN 1 AND 52428800");
                table.HasCheckConstraint("ck_listing_image_position", "\"Position\" BETWEEN 0 AND 4");
            });
            entity.HasKey(image => image.Id);
            entity.HasIndex(image => new { image.ListingId, image.Position }).IsUnique();
            entity.HasIndex(image => image.ObjectKey).IsUnique();
            entity.Property(image => image.ObjectKey).HasMaxLength(100).IsRequired();
            entity.Property(image => image.ContentType).HasMaxLength(32).IsRequired();
        });
        builder.Entity<ListingReviewEvent>(entity =>
        {
            entity.ToTable("vehicle_listing_review_events", table =>
            {
                table.HasCheckConstraint("ck_listing_review_decision",
                    "\"Decision\" IN ('Approved','Rejected')");
                table.HasCheckConstraint("ck_listing_review_reason",
                    "(\"Decision\" = 'Approved' AND \"ReasonCode\" IS NULL) OR " +
                    "(\"Decision\" = 'Rejected' AND \"ReasonCode\" IN " +
                    "('missing_information','invalid_vehicle_details','image_quality','other'))");
            });
            entity.HasKey(review => review.Id);
            entity.HasIndex(review => new { review.ListingId, review.CreatedAtUtc });
            entity.Property(review => review.Decision).HasMaxLength(16).IsRequired();
            entity.Property(review => review.ReasonCode).HasMaxLength(40);
            entity.HasOne<VehicleListing>().WithMany().HasForeignKey(review => review.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Booking>(entity =>
        {
            entity.ToTable("bookings", table =>
            {
                table.HasCheckConstraint("ck_booking_status",
                    "\"Status\" IN ('Requested','Confirmed','Declined','Cancelled')");
                table.HasCheckConstraint("ck_booking_dates",
                    "\"EndDate\" >= \"StartDate\" AND \"EndDate\" - \"StartDate\" < 60");
                table.HasCheckConstraint("ck_booking_price", "\"DailyPrice\" > 0 AND \"TotalPrice\" > 0");
                table.HasCheckConstraint("ck_booking_parties", "\"RenterUserId\" <> \"OwnerUserId\"");
                table.HasCheckConstraint("ck_booking_decision",
                    "(\"Status\" = 'Requested' AND \"DecidedAtUtc\" IS NULL) OR " +
                    "(\"Status\" <> 'Requested' AND \"DecidedAtUtc\" IS NOT NULL)");
            });
            entity.HasKey(booking => booking.Id);
            entity.Ignore(booking => booking.Days);
            entity.Property(booking => booking.Status).HasMaxLength(16).IsRequired();
            entity.Property(booking => booking.Message).HasMaxLength(Booking.MaxMessageLength);
            entity.Property(booking => booking.DailyPrice).HasPrecision(10, 2);
            entity.Property(booking => booking.TotalPrice).HasPrecision(12, 2);
            entity.Property(booking => booking.Version).IsConcurrencyToken();
            entity.HasIndex(booking => new { booking.ListingId, booking.Status, booking.StartDate });
            entity.HasIndex(booking => new { booking.RenterUserId, booking.CreatedAtUtc });
            entity.HasIndex(booking => new { booking.OwnerUserId, booking.Status, booking.StartDate });
            // Listings with booking history are archived, never deleted, so the history is kept.
            entity.HasOne<VehicleListing>().WithMany().HasForeignKey(booking => booking.ListingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        CatalogSeedData.Configure(builder);
    }
}
