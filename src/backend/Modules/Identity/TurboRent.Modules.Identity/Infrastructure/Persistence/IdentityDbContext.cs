using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TurboRent.Modules.Identity.Domain;

namespace TurboRent.Modules.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<OwnerProfile> OwnerProfiles => Set<OwnerProfile>();
    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<ApplicationSession> Sessions => Set<ApplicationSession>();
    public DbSet<SecurityAuditRecord> SecurityAuditRecords => Set<SecurityAuditRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("identity");
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users", table =>
            {
                table.HasCheckConstraint("ck_users_locale", "\"Locale\" IN ('az', 'en')");
                table.HasCheckConstraint("ck_users_status", "\"AccountStatus\" IN ('Active', 'Suspended', 'Deactivated')");
                table.HasCheckConstraint("ck_users_totp_step", "\"LastAcceptedTotpStep\" IS NULL OR \"LastAcceptedTotpStep\" >= 0");
            });
            entity.Property(user => user.Email).IsRequired();
            entity.Property(user => user.NormalizedEmail).IsRequired();
            entity.HasIndex(user => user.NormalizedEmail).HasDatabaseName("EmailIndex").IsUnique();
            entity.Property(user => user.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Locale).HasMaxLength(2).IsRequired();
            entity.Property(user => user.AccountStatus).HasMaxLength(16).IsRequired();
        });
        builder.Entity<ApplicationRole>().ToTable("roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
        builder.Entity<IdentityUserLogin<Guid>>().Property(x => x.LoginProvider).HasMaxLength(128);
        builder.Entity<IdentityUserLogin<Guid>>().Property(x => x.ProviderKey).HasMaxLength(128);
        builder.Entity<IdentityUserToken<Guid>>().Property(x => x.LoginProvider).HasMaxLength(128);
        builder.Entity<IdentityUserToken<Guid>>().Property(x => x.Name).HasMaxLength(128);

        builder.Entity<OwnerProfile>(entity =>
        {
            entity.ToTable("owner_profiles", table =>
            {
                table.HasCheckConstraint("ck_owner_status", "\"Status\" IN ('Pending','Approved','Rejected','Suspended')");
                table.HasCheckConstraint("ck_owner_review", ReviewConstraint);
            });
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.Property(x => x.ContactPhone).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.ReasonCode).HasMaxLength(64);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<CompanyProfile>(entity =>
        {
            entity.ToTable("company_profiles", table =>
            {
                table.HasCheckConstraint("ck_company_status", "\"Status\" IN ('Pending','Approved','Rejected','Suspended')");
                table.HasCheckConstraint("ck_company_review", ReviewConstraint);
                table.HasCheckConstraint("ck_company_voen", "\"Voen\" ~ '^[0-9]{10}$'");
            });
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => x.Voen).IsUnique().HasFilter("\"Status\" = 'Approved'");
            entity.Property(x => x.LegalName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Voen).HasMaxLength(10).IsRequired();
            entity.Property(x => x.ContactPhone).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.ReasonCode).HasMaxLength(64);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<ApplicationSession>(entity =>
        {
            entity.ToTable("sessions", table =>
            {
                table.HasCheckConstraint("ck_session_expiry", "\"CreatedAtUtc\" < \"IdleExpiresAtUtc\" AND \"IdleExpiresAtUtc\" <= \"AbsoluteExpiresAtUtc\" AND \"LastSeenAtUtc\" >= \"CreatedAtUtc\" AND \"LastSeenAtUtc\" < \"IdleExpiresAtUtc\"");
                table.HasCheckConstraint("ck_session_admin_mfa", "NOT \"IsAdminSession\" OR \"MfaVerifiedAtUtc\" IS NOT NULL");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => new { x.UserId, x.RevokedAtUtc });
            entity.HasIndex(x => x.AbsoluteExpiresAtUtc);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<SecurityAuditRecord>(entity =>
        {
            entity.ToTable("security_audit_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ResultCode).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TraceId).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.SubjectUserId, x.OccurredAtUtc });
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.SubjectUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<ApplicationRole>().HasData(
            Role(IdentityRoles.CustomerId, IdentityRoles.Customer),
            Role(IdentityRoles.OwnerId, IdentityRoles.IndividualOwner),
            Role(IdentityRoles.CompanyId, IdentityRoles.RentalCompany),
            Role(IdentityRoles.AdminId, IdentityRoles.Admin));
    }

    private const string ReviewConstraint =
        "(\"Status\" = 'Pending' AND \"ReviewedByUserId\" IS NULL AND \"ReviewedAtUtc\" IS NULL AND \"ReasonCode\" IS NULL) OR " +
        "(\"Status\" <> 'Pending' AND \"ReviewedByUserId\" IS NOT NULL AND \"ReviewedAtUtc\" IS NOT NULL AND \"ReviewedByUserId\" <> \"UserId\" " +
        "AND (\"Status\" = 'Approved' OR NULLIF(BTRIM(\"ReasonCode\"), '') IS NOT NULL))";

    private static ApplicationRole Role(Guid id, string name) =>
        new() { Id = id, Name = name, NormalizedName = name.ToUpperInvariant(), ConcurrencyStamp = id.ToString() };
}
