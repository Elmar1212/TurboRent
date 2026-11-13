using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Infrastructure.Persistence;

namespace TurboRent.Identity.IntegrationTests;

public class MigrationTests
{
    private const string ReviewedSqlHash = "12B167C30FFECE7B9410F244AB2AE0D8F088CA81BE7B7BFD02C364275C813BCD";
    private static async Task<IdentityDbContext> OpenContext()
    {
        var connection = Environment.GetEnvironmentVariable("TURBORENT_TEST_DATABASE")
            ?? throw new InvalidOperationException("An isolated test connection is required.");
        var parsed = new NpgsqlConnectionStringBuilder(connection);
        if (parsed.Database is null || !parsed.Database.StartsWith("turborent_identity_test_", StringComparison.Ordinal) ||
            parsed.Username is null || !parsed.Username.StartsWith("turborent_identity_test_", StringComparison.Ordinal) ||
            parsed.Host != "127.0.0.1" || parsed.Port != 55432)
            throw new InvalidOperationException("Refusing a connection outside the isolated Identity test database.");
        var db = new IdentityDbContext(new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(connection, p => p.MigrationsHistoryTable("__EFMigrationsHistory", "identity")).Options);
        await db.Database.OpenConnectionAsync();
        using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT current_database(), current_user";
        using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal(parsed.Database, reader.GetString(0));
        Assert.Equal(parsed.Username, reader.GetString(1));
        return db;
    }

    private static async Task WithRollback(Func<IdentityDbContext, Task> action)
    {
        await using var db = await OpenContext();
        await using var transaction = await db.Database.BeginTransactionAsync();
        try { await action(db); }
        finally { await transaction.RollbackAsync(); }
    }

    private static ApplicationUser User(string? email = null)
    {
        var id = Guid.NewGuid();
        email ??= $"migration-{id:N}@example.test";
        return new ApplicationUser
        {
            Id = id, UserName = email, NormalizedUserName = email.ToUpperInvariant(),
            Email = email, NormalizedEmail = email.ToUpperInvariant(),
            DisplayName = "Migration Test", Locale = "az", AccountStatus = "Active",
            CreatedAtUtc = DateTimeOffset.UtcNow, SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(), LockoutEnabled = true
        };
    }

    private static async Task ExpectPostgresError(
        IdentityDbContext db, string sqlState, string? constraint = null)
    {
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        var postgres = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(sqlState, postgres.SqlState);
        if (constraint is not null) Assert.Equal(constraint, postgres.ConstraintName);
    }

    [Fact]
    public async Task Migration_creates_only_expected_identity_tables_and_one_history_row()
    {
        await using var db = await OpenContext();
        using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT table_schema || '.' || table_name FROM information_schema.tables WHERE table_schema = 'identity' ORDER BY 1";
        var tables = new List<string>();
        await using (var reader = await command.ExecuteReaderAsync())
            while (await reader.ReadAsync()) tables.Add(reader.GetString(0));
        Assert.Equal(new[]
        {
            "identity.__EFMigrationsHistory", "identity.company_profiles", "identity.owner_profiles",
            "identity.role_claims", "identity.roles", "identity.security_audit_records",
            "identity.sessions", "identity.user_claims", "identity.user_logins",
            "identity.user_roles", "identity.user_tokens", "identity.users"
        }, tables);
        Assert.Equal(new[] { "20261025034229_InitialIdentity" }, await db.Database.GetAppliedMigrationsAsync());
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
    }

    [Fact]
    public async Task Four_roles_are_seeded_and_no_admin_user_is_assigned()
    {
        await using var db = await OpenContext();
        Assert.Equal(new[] { "Admin", "Customer", "IndividualOwner", "RentalCompany" },
            await db.Roles.OrderBy(role => role.Name).Select(role => role.Name!).ToArrayAsync());
        Assert.False(await db.Set<IdentityUserRole<Guid>>().AnyAsync(role => role.RoleId == IdentityRoles.AdminId));
    }

    [Fact]
    public Task Duplicate_normalized_email_is_rejected_by_postgres() => WithRollback(async db =>
    {
        var email = $"unique-{Guid.NewGuid():N}@example.test";
        db.Users.Add(User(email));
        await db.SaveChangesAsync();
        db.Users.Add(User(email.ToUpperInvariant()));
        await ExpectPostgresError(db, PostgresErrorCodes.UniqueViolation, "EmailIndex");
    });

    [Theory]
    [InlineData("locale", "ck_users_locale")]
    [InlineData("status", "ck_users_status")]
    public Task Invalid_account_values_are_rejected(string field, string constraint) => WithRollback(async db =>
    {
        var user = User();
        if (field == "locale") user.Locale = "xx";
        else user.AccountStatus = "Admin";
        db.Users.Add(user);
        await ExpectPostgresError(db, PostgresErrorCodes.CheckViolation, constraint);
    });

    [Fact]
    public Task Profile_requires_an_existing_user() => WithRollback(async db =>
    {
        db.OwnerProfiles.Add(new OwnerProfile
        {
            UserId = Guid.NewGuid(), ContactPhone = "+994501234567", SubmittedAtUtc = DateTimeOffset.UtcNow
        });
        await ExpectPostgresError(db, PostgresErrorCodes.ForeignKeyViolation);
    });

    [Fact]
    public Task One_owner_profile_per_user_is_enforced() => WithRollback(async db =>
    {
        var user = User();
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.OwnerProfiles.Add(new OwnerProfile { UserId = user.Id, SubmittedAtUtc = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        db.OwnerProfiles.Add(new OwnerProfile { UserId = user.Id, SubmittedAtUtc = DateTimeOffset.UtcNow });
        await ExpectPostgresError(db, PostgresErrorCodes.UniqueViolation, "IX_owner_profiles_UserId");
    });

    [Fact]
    public Task Database_rejects_self_review_even_when_domain_method_is_bypassed() => WithRollback(async db =>
    {
        var user = User();
        db.Users.Add(user);
        var profile = new OwnerProfile { UserId = user.Id, SubmittedAtUtc = DateTimeOffset.UtcNow };
        db.OwnerProfiles.Add(profile);
        await db.SaveChangesAsync();
        var exception = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE identity.owner_profiles SET \"Status\" = 'Approved', \"ReviewedByUserId\" = {user.Id}, \"ReviewedAtUtc\" = {DateTimeOffset.UtcNow} WHERE \"Id\" = {profile.Id}"));
        Assert.Equal("ck_owner_review", exception.ConstraintName);
    });

    [Fact]
    public Task Invalid_voen_is_rejected_by_postgres() => WithRollback(async db =>
    {
        var user = User();
        db.Users.Add(user);
        db.CompanyProfiles.Add(new CompanyProfile
        {
            UserId = user.Id, LegalName = "Example Test Company", Voen = "invalid", SubmittedAtUtc = DateTimeOffset.UtcNow
        });
        await ExpectPostgresError(db, PostgresErrorCodes.CheckViolation, "ck_company_voen");
    });

    [Fact]
    public Task Approved_company_voen_is_unique_but_pending_duplicates_are_allowed() => WithRollback(async db =>
    {
        var owner = User();
        var otherOwner = User();
        var reviewer = User(); // A data-only reviewer fixture, not an Admin account.
        db.Users.AddRange(owner, otherOwner, reviewer);
        var first = new CompanyProfile { UserId = owner.Id, LegalName = "Example A", Voen = "1234567890", SubmittedAtUtc = DateTimeOffset.UtcNow };
        var second = new CompanyProfile { UserId = otherOwner.Id, LegalName = "Example B", Voen = "1234567890", SubmittedAtUtc = DateTimeOffset.UtcNow };
        db.CompanyProfiles.AddRange(first, second);
        await db.SaveChangesAsync();
        first.Review(ApprovalStatus.Approved, reviewer.Id, null, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
        second.Review(ApprovalStatus.Approved, reviewer.Id, null, DateTimeOffset.UtcNow);
        await ExpectPostgresError(db, PostgresErrorCodes.UniqueViolation, "IX_company_profiles_Voen");
    });

    [Fact]
    public Task Database_rejects_admin_session_without_mfa() => WithRollback(async db =>
    {
        var user = User();
        db.Users.Add(user);
        var session = ApplicationSession.Create(user.Id, false, false, DateTimeOffset.UtcNow);
        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        var exception = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE identity.sessions SET \"IsAdminSession\" = TRUE WHERE \"Id\" = {session.Id}"));
        Assert.Equal("ck_session_admin_mfa", exception.ConstraintName);
    });

    [Fact]
    public Task Database_rejects_invalid_session_expiry() => WithRollback(async db =>
    {
        var user = User();
        db.Users.Add(user);
        var session = ApplicationSession.Create(user.Id, false, false, DateTimeOffset.UtcNow);
        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        var exception = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE identity.sessions SET \"IdleExpiresAtUtc\" = \"CreatedAtUtc\" WHERE \"Id\" = {session.Id}"));
        Assert.Equal("ck_session_expiry", exception.ConstraintName);
    });

    [Fact]
    public Task Stale_session_update_is_rejected_by_optimistic_concurrency() => WithRollback(async db =>
    {
        var now = DateTimeOffset.UtcNow;
        var user = User();
        db.Users.Add(user);
        var session = ApplicationSession.Create(user.Id, false, false, now);
        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        await using var other = new IdentityDbContext(new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(db.Database.GetDbConnection()).Options);
        await other.Database.UseTransactionAsync(db.Database.CurrentTransaction!.GetDbTransaction());
        var stale = await other.Sessions.SingleAsync(s => s.Id == session.Id);
        session.Revoke(now.AddSeconds(1));
        await db.SaveChangesAsync();
        stale.Touch(now.AddSeconds(2));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => other.SaveChangesAsync());
    });

    [Fact]
    public Task Pending_profiles_do_not_automatically_grant_privileged_roles() => WithRollback(async db =>
    {
        var user = User();
        db.Users.Add(user);
        db.OwnerProfiles.Add(new OwnerProfile { UserId = user.Id, SubmittedAtUtc = DateTimeOffset.UtcNow });
        db.CompanyProfiles.Add(new CompanyProfile { UserId = user.Id, LegalName = "Example Test", Voen = "0987654321", SubmittedAtUtc = DateTimeOffset.UtcNow });
        db.Set<IdentityUserRole<Guid>>().Add(new() { UserId = user.Id, RoleId = IdentityRoles.CustomerId });
        await db.SaveChangesAsync();
        Assert.Equal(new[] { IdentityRoles.CustomerId },
            await db.Set<IdentityUserRole<Guid>>().Where(role => role.UserId == user.Id).Select(role => role.RoleId).ToArrayAsync());
    });

    [Fact]
    public Task Audit_requires_valid_user_references() => WithRollback(async db =>
    {
        db.SecurityAuditRecords.Add(new SecurityAuditRecord
        {
            SubjectUserId = Guid.NewGuid(), EventCode = "migration_test", ResultCode = "test", OccurredAtUtc = DateTimeOffset.UtcNow
        });
        await ExpectPostgresError(db, PostgresErrorCodes.ForeignKeyViolation);
    });

    [Fact]
    public async Task Reapplying_reviewed_sql_preserves_schema_and_all_existing_rows()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "TurboRent.slnx"))) root = root.Parent;
        Assert.NotNull(root);
        var sqlPath = Path.Combine(root.FullName, "docs", "migrations", "InitialIdentity.sql");
        var bytes = await File.ReadAllBytesAsync(sqlPath);
        Assert.Equal(ReviewedSqlHash, Convert.ToHexString(SHA256.HashData(bytes)));

        await using var db = await OpenContext();
        // Retained synthetic marker makes preservation test meaningful; it has no password or Admin role.
        var marker = User();
        db.Users.Add(marker);
        await db.SaveChangesAsync();
        var schemaBefore = await SchemaSnapshot(db);
        var rowsBefore = await DataSnapshot(db);
        using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = await File.ReadAllTextAsync(sqlPath); // StreamReader strips the UTF-8 BOM; reviewed bytes remain unchanged.
        await command.ExecuteNonQueryAsync();
        Assert.Equal(schemaBefore, await SchemaSnapshot(db));
        Assert.Equal(rowsBefore, await DataSnapshot(db));
        Assert.True(await db.Users.AnyAsync(user => user.Id == marker.Id));
        Assert.Equal(4, await db.Roles.CountAsync());
        Assert.Single(await db.Database.GetAppliedMigrationsAsync());
    }

    private static async Task<string> SchemaSnapshot(IdentityDbContext db)
    {
        using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT kind, definition FROM (
              SELECT 'column' AS kind, table_name || ':' || column_name || ':' || ordinal_position || ':' || data_type || ':' || is_nullable || ':' || COALESCE(column_default,'') AS definition
              FROM information_schema.columns WHERE table_schema = 'identity'
              UNION ALL
              SELECT 'constraint', c.relname || ':' || con.conname || ':' || pg_get_constraintdef(con.oid)
              FROM pg_constraint con JOIN pg_class c ON c.oid = con.conrelid JOIN pg_namespace n ON n.oid = c.relnamespace WHERE n.nspname = 'identity'
              UNION ALL
              SELECT 'index', tablename || ':' || indexname || ':' || indexdef FROM pg_indexes WHERE schemaname = 'identity'
            ) definitions ORDER BY kind, definition
            """;
        var result = new List<string>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add(reader.GetString(0) + ":" + reader.GetString(1));
        return string.Join("\n", result);
    }

    private static async Task<string> DataSnapshot(IdentityDbContext db)
    {
        var tables = db.Model.GetEntityTypes().Select(entity => entity.GetTableName()!).Distinct()
            .Append("__EFMigrationsHistory").Order().ToArray();
        var result = new List<string>();
        foreach (var table in tables)
        {
            // Names originate exclusively from the fixed EF model, never request input.
            using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"SELECT COALESCE(string_agg(to_jsonb(t)::text, E'\\n' ORDER BY to_jsonb(t)::text), '') FROM identity.\"{table}\" t";
            result.Add(table + ":" + (string)(await command.ExecuteScalarAsync())!);
        }
        return string.Join("\n", result);
    }
}
