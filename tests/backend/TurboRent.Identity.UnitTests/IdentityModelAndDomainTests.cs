using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Infrastructure.Persistence;

namespace TurboRent.Identity.UnitTests;

public class IdentityModelTests
{
    private static IdentityDbContext CreateContext() => new(new DbContextOptionsBuilder<IdentityDbContext>()
        .UseNpgsql("Host=127.0.0.1;Database=design_only").Options);

    [Fact]
    public void Every_identity_table_is_isolated_in_identity_schema()
    {
        using var db = CreateContext();
        Assert.All(db.Model.GetEntityTypes(), entity => Assert.Equal("identity", entity.GetSchema()));
    }

    [Fact]
    public void Email_uniqueness_is_enforced_by_database_model()
    {
        using var db = CreateContext();
        var user = db.Model.FindEntityType(typeof(ApplicationUser))!;
        Assert.Contains(user.GetIndexes(), index => index.IsUnique &&
            index.Properties.Select(property => property.Name).SequenceEqual(["NormalizedEmail"]));
    }

    [Fact]
    public void Only_roles_are_seeded_never_admin_users_or_assignments()
    {
        using var db = CreateContext();
        var model = db.GetService<IDesignTimeModel>().Model;
        var seeded = model.GetEntityTypes().Where(entity => entity.GetSeedData().Any()).ToList();
        var role = Assert.Single(seeded);
        Assert.Equal(typeof(ApplicationRole), role.ClrType);
        Assert.Equal(4, role.GetSeedData().Count());
    }

    [Fact]
    public void Sensitive_security_state_has_concurrency_control()
    {
        using var db = CreateContext();
        foreach (var type in new[] { typeof(OwnerProfile), typeof(CompanyProfile), typeof(ApplicationSession) })
            Assert.True(db.Model.FindEntityType(type)!.FindProperty("Version")!.IsConcurrencyToken);
        Assert.True(db.Model.FindEntityType(typeof(ApplicationUser))!.FindProperty("ConcurrencyStamp")!.IsConcurrencyToken);
    }

    [Fact]
    public void No_refresh_token_table_exists_for_cookie_authentication()
    {
        using var db = CreateContext();
        Assert.DoesNotContain(db.Model.GetEntityTypes(), entity =>
            entity.GetTableName()!.Contains("refresh", StringComparison.OrdinalIgnoreCase));
    }
}

public class ApprovalTests
{
    [Fact]
    public void Applicant_cannot_approve_their_own_profile()
    {
        var owner = new OwnerProfile { UserId = Guid.NewGuid() };
        Assert.Throws<InvalidOperationException>(() =>
            owner.Review(ApprovalStatus.Approved, owner.UserId, null, DateTimeOffset.UtcNow));
        Assert.Equal(ApprovalStatus.Pending, owner.Status);
    }

    [Fact]
    public void Approval_then_suspension_requires_reason_and_changes_concurrency_version()
    {
        var company = new CompanyProfile { UserId = Guid.NewGuid() };
        var reviewer = Guid.NewGuid();
        var original = company.Version;
        company.Review(ApprovalStatus.Approved, reviewer, null, DateTimeOffset.UtcNow);
        Assert.NotEqual(original, company.Version);
        Assert.Throws<ArgumentException>(() =>
            company.Review(ApprovalStatus.Suspended, reviewer, " ", DateTimeOffset.UtcNow));
        Assert.Equal(ApprovalStatus.Approved, company.Status);
        company.Review(ApprovalStatus.Suspended, reviewer, "policy_violation", DateTimeOffset.UtcNow);
        Assert.Equal(ApprovalStatus.Suspended, company.Status);
    }

    [Theory]
    [InlineData(ApprovalStatus.Pending)]
    [InlineData(ApprovalStatus.Suspended)]
    public void Pending_profile_cannot_skip_valid_review_transitions(ApprovalStatus next)
    {
        var owner = new OwnerProfile { UserId = Guid.NewGuid() };
        Assert.Throws<InvalidOperationException>(() =>
            owner.Review(next, Guid.NewGuid(), "reason", DateTimeOffset.UtcNow));
    }
}

public class SessionTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Admin_session_cannot_be_created_without_mfa() =>
        Assert.Throws<InvalidOperationException>(() => ApplicationSession.Create(Guid.NewGuid(), true, false, Now));

    [Theory]
    [InlineData(false, 30)]
    [InlineData(true, 15)]
    public void Idle_session_expires_at_boundary(bool admin, int minutes)
    {
        var session = ApplicationSession.Create(Guid.NewGuid(), admin, admin, Now);
        Assert.True(session.IsActive(Now.AddMinutes(minutes).AddTicks(-1)));
        Assert.False(session.IsActive(Now.AddMinutes(minutes)));
        Assert.Throws<InvalidOperationException>(() => session.Touch(Now.AddMinutes(minutes)));
    }

    [Fact]
    public void Sliding_activity_never_extends_absolute_admin_lifetime()
    {
        var session = ApplicationSession.Create(Guid.NewGuid(), true, true, Now);
        for (var minute = 10; minute < 480; minute += 10)
            session.Touch(Now.AddMinutes(minute));
        Assert.Equal(Now.AddHours(8), session.IdleExpiresAtUtc);
        Assert.False(session.IsActive(Now.AddHours(8)));
    }

    [Fact]
    public void Revocation_is_immediate_and_cannot_be_renewed()
    {
        var session = ApplicationSession.Create(Guid.NewGuid(), false, false, Now);
        session.Revoke(Now.AddMinutes(1));
        Assert.False(session.IsActive(Now.AddMinutes(2)));
        Assert.Throws<InvalidOperationException>(() => session.Touch(Now.AddMinutes(2)));
    }
}
