using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Infrastructure.Email;
using TurboRent.Modules.Identity.Infrastructure.Persistence;
using TurboRent.Modules.Catalog.Domain;
using TurboRent.Modules.Catalog.Infrastructure.Persistence;
using TurboRent.Modules.Catalog.Infrastructure.Storage;
using TurboRent.Modules.Identity.Infrastructure.Security;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace TurboRent.Identity.IntegrationTests;

public partial class RegistrationTests
{
    private const string Password = "Test only long passphrase 2026";
    private static string Email() => $"registration-{Guid.NewGuid():N}@example.test";

    private class Factory : WebApplicationFactory<Program>
    {
        public string Mailbox { get; } = Path.Combine(Path.GetTempPath(), "turborent-mail-test-" + Guid.NewGuid().ToString("N"));
        public string? MailboxPathOverride { get; init; }
        public bool Expired { get; init; }
        public bool MisconfiguredDatabase { get; init; }
        public int IpLimit { get; init; } = 1000;
        public string EnvironmentName { get; init; } = "Testing";
        public TimeProvider? Time { get; init; }
        public Dictionary<string, string?> ExtraSettings { get; init; } = [];
        public readonly List<string> Logs = [];
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var connection = Environment.GetEnvironmentVariable("TURBORENT_TEST_DATABASE") ?? throw new InvalidOperationException("Isolated test database required.");
            var parsed = new NpgsqlConnectionStringBuilder(connection);
            if (parsed.Database?.StartsWith("turborent_identity_test_", StringComparison.Ordinal) != true)
                throw new InvalidOperationException("Refusing main database.");
            builder.UseEnvironment(EnvironmentName).ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database"] = MisconfiguredDatabase
                        ? new NpgsqlConnectionStringBuilder(connection) { Database = "turborent" }.ConnectionString : connection,
                    ["Identity:MailboxPath"] = MailboxPathOverride ?? Mailbox,
                    ["Identity:FrontendOrigin"] = "https://127.0.0.1:5174",
                    ["Identity:IpPermitLimit"] = IpLimit.ToString(),
                    ["AdminBootstrap:Secret"] = new string('b', 64)
                }).AddInMemoryCollection(ExtraSettings));
            builder.ConfigureServices(services =>
            {
                if (Expired) services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromSeconds(-1));
                services.RemoveAll<IVehicleImageStorage>();
                services.AddSingleton<IVehicleImageStorage, MemoryVehicleImageStorage>();
                if (Time is not null)
                {
                    services.RemoveAll<TimeProvider>();
                    services.AddSingleton(Time);
                }
            });
            builder.ConfigureLogging(logging => logging.AddProvider(new CaptureLogs(Logs)));
        }

        private sealed class MemoryVehicleImageStorage : IVehicleImageStorage
        {
            private readonly System.Collections.Concurrent.ConcurrentDictionary<string, StoredImage> _images = new();
            private sealed record StoredImage(byte[] Content, string ContentType);

            public async Task PutAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
            {
                await using var buffer = new MemoryStream();
                await content.CopyToAsync(buffer, cancellationToken);
                _images[key] = new StoredImage(buffer.ToArray(), contentType);
            }

            public Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken) =>
                Task.FromResult<Stream?>(_images.TryGetValue(key, out var image)
                    ? new MemoryStream(image.Content, writable: false) : null);

            public Task DeleteAsync(string key, CancellationToken cancellationToken)
            {
                _images.TryRemove(key, out _);
                return Task.CompletedTask;
            }
        }
        public HttpClient Client() => CreateClient(new WebApplicationFactoryClientOptions
        { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = true });
        public IdentityDbContext Db(IServiceScope scope) => scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        public CatalogDbContext CatalogDb(IServiceScope scope) => scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        public async Task<string> CreateAdmin(string email)
        {
            using var scope = Services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var totp = scope.ServiceProvider.GetRequiredService<AdminTotpService>();
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "Admin Test",
                Locale = "az",
                AccountStatus = "Active",
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            Assert.True((await users.CreateAsync(user, Password)).Succeeded);
            Assert.True((await users.AddToRoleAsync(user, IdentityRoles.Admin)).Succeeded);
            var secret = AdminTotpService.GenerateSecret();
            Assert.True((await totp.SetSecretAsync(user, secret)).Succeeded);
            return secret;
        }
        public async Task DeleteAdmin(string email)
        {
            using var scope = Services.CreateScope();
            var db = Db(scope);
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            if (user is null) return;
            await db.SecurityAuditRecords.Where(record =>
                record.ActorUserId == user.Id || record.SubjectUserId == user.Id).ExecuteDeleteAsync();
            await db.Sessions.Where(session => session.UserId == user.Id).ExecuteDeleteAsync();
            Assert.True((await userManager.DeleteAsync(user)).Succeeded);
        }
        public async Task<JsonElement> Message(string email)
        {
            foreach (var file in Directory.GetFiles(MailboxPathOverride ?? Mailbox, "*.json"))
            {
                var message = JsonDocument.Parse(await File.ReadAllTextAsync(file)).RootElement.Clone();
                if (message.GetProperty("email").GetString() == email) return message;
            }
            throw new InvalidOperationException("Expected local message missing.");
        }
    }

    private sealed class CaptureLogs(List<string> entries) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new Capture(entries);
        public void Dispose() { }
        private sealed class Capture(List<string> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel level) => true;
            public void Log<TState>(LogLevel level, EventId id, TState state, Exception? exception, Func<TState, Exception?, string> format)
            { lock (entries) entries.Add(format(state, exception)); }
        }
    }

    private static object Request(string email, string type = "customer") => new
    {
        email, password = Password, displayName = "Registration Test", locale = "az", accountType = type,
        contactPhone = "+994501234567", legalName = "Example Test Company", voen = "1234567890"
    };
    private static async Task Csrf(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/csrf");
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", response.GetProperty("requestToken").GetString());
    }

    private static object Verification(JsonElement message)
    {
        var fragment = new Uri(message.GetProperty("verificationUrl").GetString()!).Fragment.TrimStart('#');
        var values = QueryHelpers.ParseQuery(fragment);
        return new { userId = values["userId"].ToString(), token = values["token"].ToString() };
    }
    private static string Totp(string base32, DateTimeOffset? now = null)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var key = new List<byte>();
        var buffer = 0;
        var bits = 0;
        foreach (var character in base32)
        {
            buffer = (buffer << 5) | alphabet.IndexOf(character);
            bits += 5;
            if (bits < 8) continue;
            bits -= 8;
            key.Add((byte)(buffer >> bits));
        }
        var counter = (now ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds() / 30;
        Span<byte> message = stackalloc byte[8];
        System.Buffers.Binary.BinaryPrimitives.WriteInt64BigEndian(message, counter);
        var hash = HMACSHA1.HashData(key.ToArray(), message);
        var offset = hash[^1] & 15;
        var binary = ((hash[offset] & 0x7f) << 24) | (hash[offset + 1] << 16) |
            (hash[offset + 2] << 8) | hash[offset + 3];
        return (binary % 1_000_000).ToString("D6", System.Globalization.CultureInfo.InvariantCulture);
    }

    [Theory]
    [InlineData("customer")]
    [InlineData("owner")]
    [InlineData("company")]
    public async Task Registration_hashes_password_and_assigns_only_customer_with_pending_profile(string type)
    {
        await using var factory = new Factory();
        using var client = factory.Client();
        await Csrf(client);
        var email = Email();
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", Request(email, type));
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("token", body, StringComparison.OrdinalIgnoreCase);
        using var scope = factory.Services.CreateScope();
        var db = factory.Db(scope);
        var user = await db.Users.SingleAsync(u => u.Email == email);
        Assert.False(user.EmailConfirmed);
        Assert.NotEqual(Password, user.PasswordHash);
        Assert.NotEqual(PasswordVerificationResult.Failed,
            new PasswordHasher<ApplicationUser>().VerifyHashedPassword(user, user.PasswordHash!, Password));
        Assert.Equal(new[] { IdentityRoles.CustomerId },
            await db.Set<IdentityUserRole<Guid>>().Where(r => r.UserId == user.Id).Select(r => r.RoleId).ToArrayAsync());
        Assert.Equal(type == "owner" ? 1 : 0, await db.OwnerProfiles.CountAsync(p => p.UserId == user.Id && p.Status == ApprovalStatus.Pending));
        Assert.Equal(type == "company" ? 1 : 0, await db.CompanyProfiles.CountAsync(p => p.UserId == user.Id && p.Status == ApprovalStatus.Pending));
        Assert.True(await db.SecurityAuditRecords.AnyAsync(a => a.SubjectUserId == user.Id && a.EventCode == "registration"));
        Assert.False(await db.Sessions.AnyAsync(s => s.UserId == user.Id));
        var message = await factory.Message(email);
        var url = new Uri(message.GetProperty("verificationUrl").GetString()!);
        Assert.Equal("", url.Query); // Secret is a fragment, never in server request URLs.
        Assert.DoesNotContain(factory.Logs, line => line.Contains(Password, StringComparison.Ordinal) || line.Contains(url.Fragment, StringComparison.Ordinal));
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            Assert.DoesNotContain(cookies, cookie => !cookie.Contains("Csrf", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Registration_reports_mailbox_write_failure_and_resend_recovers()
    {
        var blockedPath = Path.Combine(Path.GetTempPath(), "turborent-mail-blocked-" + Guid.NewGuid().ToString("N"));
        await File.WriteAllTextAsync(blockedPath, "not a directory");
        try
        {
            await using var factory = new Factory { MailboxPathOverride = blockedPath };
            using var client = factory.Client();
            await Csrf(client);
            var email = Email();
            var registration = await client.PostAsJsonAsync("/api/v1/auth/register", Request(email));
            Assert.Equal(HttpStatusCode.ServiceUnavailable, registration.StatusCode);
            Assert.Equal("local_mailbox_unavailable",
                (await registration.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());

            using (var scope = factory.Services.CreateScope())
                Assert.False((await factory.Db(scope).Users.SingleAsync(user => user.Email == email)).EmailConfirmed);

            File.Delete(blockedPath);
            Directory.CreateDirectory(blockedPath);
            var resend = await client.PostAsJsonAsync("/api/v1/auth/email/resend", new { email });
            Assert.Equal(HttpStatusCode.Accepted, resend.StatusCode);
            Assert.Equal(email, (await factory.Message(email)).GetProperty("email").GetString());
        }
        finally
        {
            if (File.Exists(blockedPath)) File.Delete(blockedPath);
            if (Directory.Exists(blockedPath)) Directory.Delete(blockedPath, recursive: true);
        }
    }

    [Fact]
    public async Task Duplicate_and_concurrent_duplicate_have_generic_response_and_one_user()
    {
        await using var factory = new Factory();
        using var a = factory.Client();
        using var b = factory.Client();
        await Csrf(a); await Csrf(b);
        var email = Email();
        var responses = await Task.WhenAll(a.PostAsJsonAsync("/api/v1/auth/register", Request(email)), b.PostAsJsonAsync("/api/v1/auth/register", Request(email)));
        Assert.All(responses, response => Assert.Equal(HttpStatusCode.Accepted, response.StatusCode));
        Assert.Equal(await responses[0].Content.ReadAsStringAsync(), await responses[1].Content.ReadAsStringAsync());
        var duplicate = await a.PostAsJsonAsync("/api/v1/auth/register", Request(email.ToUpperInvariant()));
        Assert.Equal(HttpStatusCode.Accepted, duplicate.StatusCode);
        using var scope = factory.Services.CreateScope();
        Assert.Equal(1, await factory.Db(scope).Users.CountAsync(u => u.NormalizedEmail == email.ToUpperInvariant()));
    }

    [Fact]
    public async Task Missing_csrf_rejects_registration_without_database_writes()
    {
        await using var factory = new Factory();
        using var client = factory.Client();
        var email = Email();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/auth/register", Request(email))).StatusCode);
        using var scope = factory.Services.CreateScope();
        Assert.False(await factory.Db(scope).Users.AnyAsync(u => u.Email == email));
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("invalid")]
    public async Task Invalid_account_type_is_rejected(string type)
    {
        await using var factory = new Factory(); using var client = factory.Client(); await Csrf(client);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/auth/register", Request(Email(), type))).StatusCode);
    }

    [Fact]
    public async Task Role_injection_is_rejected()
    {
        await using var factory = new Factory(); using var client = factory.Client(); await Csrf(client);
        var payload = JsonSerializer.SerializeToNode(Request(Email()))!.AsObject();
        payload["roles"] = JsonSerializer.SerializeToNode(new[] { "Admin" });
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/auth/register", payload)).StatusCode);
    }

    [Fact]
    public async Task Valid_verification_confirms_email_and_is_idempotent_without_session()
    {
        await using var factory = new Factory(); using var client = factory.Client(); await Csrf(client);
        var email = Email();
        await client.PostAsJsonAsync("/api/v1/auth/register", Request(email));
        var payload = Verification(await factory.Message(email));
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/email/verify", payload)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/email/verify", payload)).StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = factory.Db(scope); var user = await db.Users.SingleAsync(u => u.Email == email);
        Assert.True(user.EmailConfirmed);
        Assert.Equal(1, await db.SecurityAuditRecords.CountAsync(a => a.SubjectUserId == user.Id && a.EventCode == "email_verification"));
        Assert.False(await db.Sessions.AnyAsync(s => s.UserId == user.Id));
    }

    [Fact]
    public async Task Invalid_expired_and_wrong_purpose_tokens_are_rejected()
    {
        await using var factory = new Factory { Expired = true }; using var client = factory.Client(); await Csrf(client);
        var email = Email();
        await client.PostAsJsonAsync("/api/v1/auth/register", Request(email));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/auth/email/verify", Verification(await factory.Message(email)))).StatusCode);
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = (await users.FindByEmailAsync(email))!;
        var resetToken = await users.GeneratePasswordResetTokenAsync(user);
        var encoded = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(resetToken));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/auth/email/verify", new { userId = user.Id, token = encoded })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/auth/email/verify", new { userId = user.Id, token = "invalid" })).StatusCode);
    }

    [Fact]
    public async Task Resend_is_generic_and_account_rate_limited()
    {
        await using var factory = new Factory(); using var client = factory.Client(); await Csrf(client);
        var email = Email();
        await client.PostAsJsonAsync("/api/v1/auth/register", Request(email));
        var first = await client.PostAsJsonAsync("/api/v1/auth/email/resend", new { email });
        var unknown = await client.PostAsJsonAsync("/api/v1/auth/email/resend", new { email = Email() });
        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        Assert.Equal(await first.Content.ReadAsStringAsync(), await unknown.Content.ReadAsStringAsync());
        await client.PostAsJsonAsync("/api/v1/auth/email/resend", new { email });
        await client.PostAsJsonAsync("/api/v1/auth/email/resend", new { email });
        var limited = await client.PostAsJsonAsync("/api/v1/auth/email/resend", new { email });
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.NotNull(limited.Headers.RetryAfter);
    }

    [Fact]
    public async Task Ip_limit_is_applied()
    {
        await using var factory = new Factory { IpLimit = 1 }; using var client = factory.Client(); await Csrf(client);
        await client.PostAsJsonAsync("/api/v1/auth/email/resend", new { email = Email() });
        Assert.Equal(HttpStatusCode.TooManyRequests,
            (await client.PostAsJsonAsync("/api/v1/auth/email/resend", new { email = Email() })).StatusCode);
    }

    [Fact]
    public async Task Login_attempts_are_limited_to_three_per_two_minutes_and_lockout_is_two_minutes()
    {
        await using var factory = new Factory();
        Assert.Equal(TimeSpan.FromMinutes(2),
            factory.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<IdentityOptions>>()
                .Value.Lockout.DefaultLockoutTimeSpan);
        using var client = factory.Client();
        await Csrf(client);
        var email = Email();
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/login",
                new { email, password = Password });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        var limited = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password = Password });
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.InRange(limited.Headers.RetryAfter!.Delta!.Value.TotalSeconds, 1, 120);
    }

    [Fact]
    public async Task Plain_http_is_rejected_and_production_has_no_local_registration_routes()
    {
        await using var factory = new Factory(); using var client = factory.Client();
        client.BaseAddress = new Uri("http://localhost");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/auth/csrf")).StatusCode);
        await using var production = new Factory { EnvironmentName = "Production" }; using var productionClient = production.Client();
        Assert.Equal(HttpStatusCode.NotFound, (await productionClient.GetAsync("/api/v1/auth/csrf")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await productionClient.PostAsJsonAsync("/api/v1/auth/register", Request(Email()))).StatusCode);
    }

    [Fact]
    public async Task Profile_failure_rolls_back_user_role_and_audit()
    {
        // Force a storage failure after UserManager has inserted the user and role:
        // replace the email service is insufficient (it runs after commit), so use an EF interceptor.
        await using var factory = new FaultFactory(); using var client = factory.Client(); await Csrf(client);
        var email = Email();
        using var beforeScope = factory.Services.CreateScope();
        var before = factory.Db(beforeScope);
        var userCount = await before.Users.CountAsync();
        var roleCount = await before.Set<IdentityUserRole<Guid>>().CountAsync();
        var profileCount = await before.OwnerProfiles.CountAsync();
        var auditCount = await before.SecurityAuditRecords.CountAsync();
        Assert.Equal(HttpStatusCode.InternalServerError, (await client.PostAsJsonAsync("/api/v1/auth/register", Request(email, "owner"))).StatusCode);
        using var scope = factory.Services.CreateScope();
        var after = factory.Db(scope);
        Assert.False(await after.Users.AnyAsync(u => u.Email == email));
        Assert.Equal(userCount, await after.Users.CountAsync());
        Assert.Equal(roleCount, await after.Set<IdentityUserRole<Guid>>().CountAsync());
        Assert.Equal(profileCount, await after.OwnerProfiles.CountAsync());
        Assert.Equal(auditCount, await after.SecurityAuditRecords.CountAsync());
    }

    [Fact]
    public async Task Wrong_user_and_wrong_purpose_fail_with_normal_token_lifetime()
    {
        await using var factory = new Factory(); using var client = factory.Client(); await Csrf(client);
        var email = Email();
        await client.PostAsJsonAsync("/api/v1/auth/register", Request(email));
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = (await users.FindByEmailAsync(email))!;
        var resetToken = await users.GeneratePasswordResetTokenAsync(user);
        var encoded = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(resetToken));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/auth/email/verify", new { userId = user.Id, token = encoded })).StatusCode);
        var emailToken = await users.GenerateEmailConfirmationTokenAsync(user);
        var emailEncoded = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(emailToken));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/auth/email/verify", new { userId = Guid.NewGuid(), token = emailEncoded })).StatusCode);
    }

    [Fact]
    public async Task Cross_client_csrf_token_is_rejected_and_cookie_is_secure()
    {
        await using var factory = new Factory(); using var first = factory.Client(); using var second = factory.Client();
        var csrfResponse = await first.GetAsync("/api/v1/auth/csrf");
        var cookie = Assert.Single(csrfResponse.Headers.GetValues("Set-Cookie"));
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("domain=", cookie, StringComparison.OrdinalIgnoreCase);
        var csrf = await csrfResponse.Content.ReadFromJsonAsync<JsonElement>();
        second.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("requestToken").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, (await second.PostAsJsonAsync("/api/v1/auth/register", Request(Email()))).StatusCode);
    }

    [Theory]
    [InlineData(14)]
    [InlineData(129)]
    public async Task Invalid_password_length_is_rejected_before_persistence(int length)
    {
        await using var factory = new Factory(); using var client = factory.Client(); await Csrf(client);
        var email = Email();
        var payload = JsonSerializer.SerializeToNode(Request(email))!.AsObject();
        payload["password"] = new string('a', length);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/auth/register", payload)).StatusCode);
        using var scope = factory.Services.CreateScope();
        Assert.False(await factory.Db(scope).Users.AnyAsync(u => u.Email == email));
    }

    [Fact]
    public async Task Large_requests_are_rejected_and_login_requires_valid_credentials()
    {
        await using var factory = new Factory(); using var client = factory.Client(); await Csrf(client);
        var payload = JsonSerializer.SerializeToNode(Request(Email()))!.AsObject();
        payload["displayName"] = new string('x', 17000);
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        content.Headers.ContentLength = System.Text.Encoding.UTF8.GetByteCount(json);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, (await client.PostAsync("/api/v1/auth/register", content)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/v1/auth/login", new { email = Email(), password = Password })).StatusCode);
    }

    [Fact]
    public async Task Verified_user_can_login_and_logout_revokes_the_server_session()
    {
        await using var factory = new Factory();
        using var client = factory.Client();
        var email = Email();
        await Csrf(client);
        Assert.Equal(HttpStatusCode.Accepted,
            (await client.PostAsJsonAsync("/api/v1/auth/register", Request(email))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = Password })).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.PostAsJsonAsync("/api/v1/auth/email/verify", Verification(await factory.Message(email)))).StatusCode);

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = Password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var sessionCookie = Assert.Single(login.Headers.GetValues("Set-Cookie"));
        Assert.Contains("__Host-TurboRent.Session=", sessionCookie, StringComparison.Ordinal);
        Assert.Contains("secure", sessionCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", sessionCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", sessionCookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("domain=", sessionCookie, StringComparison.OrdinalIgnoreCase);

        var current = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/session");
        Assert.Equal("Registration Test", current.GetProperty("displayName").GetString());
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/v1/auth/logout", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.GetAsync("/api/v1/auth/session")).StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = factory.Db(scope);
        var userId = await db.Users.Where(user => user.Email == email).Select(user => user.Id).SingleAsync();
        var session = await db.Sessions.SingleAsync(item => item.UserId == userId);
        Assert.NotNull(session.RevokedAtUtc);
    }

    [Fact]
    public async Task Incorrect_password_does_not_create_a_session()
    {
        await using var factory = new Factory();
        using var client = factory.Client();
        var email = Email();
        await Csrf(client);
        Assert.Equal(HttpStatusCode.Accepted,
            (await client.PostAsJsonAsync("/api/v1/auth/register", Request(email))).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.PostAsJsonAsync("/api/v1/auth/email/verify", Verification(await factory.Message(email)))).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Incorrect passphrase 2026" })).StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = factory.Db(scope);
        var userId = await db.Users.Where(user => user.Email == email).Select(user => user.Id).SingleAsync();
        Assert.False(await db.Sessions.AnyAsync(session => session.UserId == userId));
    }

    [Fact]
    public async Task Admin_login_requires_a_valid_non_replayed_authenticator_code()
    {
        await using var factory = new Factory();
        using var client = factory.Client();
        var email = Email();
        var secret = await factory.CreateAdmin(email);
        await Csrf(client);

        var passwordOnly = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password = Password });
        Assert.Equal(HttpStatusCode.Forbidden, passwordOnly.StatusCode);

        var code = Totp(secret);
        var login = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password = Password, totpCode = code });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.True((await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("isAdmin").GetBoolean());
        var session = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/session");
        Assert.True(session.GetProperty("isAdmin").GetBoolean());

        var replay = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password = Password, totpCode = code });
        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
        Assert.Equal("mfa_invalid", (await replay.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("code").GetString());
        await factory.DeleteAdmin(email);
    }

    [Fact]
    public async Task Admin_bootstrap_rejects_an_incorrect_local_secret_without_creating_an_account()
    {
        await using var factory = new Factory();
        using var client = factory.Client();
        var email = Email();
        var response = await client.PostAsJsonAsync("/internal/admin/bootstrap", new
        {
            bootstrapSecret = new string('x', 64),
            email,
            password = Password,
            displayName = "Bootstrap Test",
            locale = "az"
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        Assert.False(await factory.Db(scope).Users.AnyAsync(user => user.Email == email));
    }

    [Fact]
    public async Task Verified_mfa_admin_can_review_pending_listings_and_public_catalog_only_exposes_approved()
    {
        await using var factory = new Factory();
        using var client = factory.Client();
        var email = Email();
        var totpSecret = await factory.CreateAdmin(email);
        var ownerEmail = Email();
        Guid ownerId;
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var owner = new ApplicationUser
            {
                UserName = ownerEmail,
                Email = ownerEmail,
                EmailConfirmed = true,
                DisplayName = "Listing Owner Test",
                Locale = "az",
                AccountStatus = "Active",
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            Assert.True((await users.CreateAsync(owner, Password)).Succeeded);
            ownerId = owner.Id;
        }
        Guid adminId;
        using (var scope = factory.Services.CreateScope())
            adminId = await factory.Db(scope).Users.Where(user => user.Email == email)
                .Select(user => user.Id).SingleAsync();
        await Csrf(client);
        var login = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password = Password, totpCode = Totp(totpSecret) });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var approvedId = Guid.NewGuid();
        var rejectedId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var imageBytes = Encoding.UTF8.GetBytes("private admin preview");
        using (var scope = factory.Services.CreateScope())
        {
            var db = factory.CatalogDb(scope);
            db.Listings.AddRange(
                new VehicleListing
                {
                    Id = approvedId, OwnerUserId = ownerId, ModelId = 1, Year = 2022, City = "baku",
                    DailyPrice = 80, Transmission = "automatic", FuelType = "hybrid",
                    Seats = 5, DescriptionAz = "Elan təsviri", DescriptionEn = "Listing description",
                    Status = "Pending", CreatedAtUtc = DateTimeOffset.UtcNow
                },
                new VehicleListing
                {
                    Id = rejectedId, ModelId = 4, Year = 2021, City = "ganja",
                    DailyPrice = 70, Transmission = "manual", FuelType = "petrol",
                    Seats = 5, DescriptionAz = "Rədd ediləcək", DescriptionEn = "To reject",
                    Status = "Pending", CreatedAtUtc = DateTimeOffset.UtcNow.AddSeconds(1)
                });
            db.ListingImages.Add(new VehicleListingImage
            {
                Id = imageId, ListingId = approvedId, ObjectKey = $"{approvedId:N}/image",
                ContentType = "image/png", Length = imageBytes.Length, Position = 0
            });
            await db.SaveChangesAsync();
            await scope.ServiceProvider.GetRequiredService<IVehicleImageStorage>().PutAsync(
                $"{approvedId:N}/image", new MemoryStream(imageBytes), "image/png", CancellationToken.None);
        }

        // The shared test database may hold older pending listings, so search every page.
        JsonElement? found = null;
        for (var page = 1; found is null; page++)
        {
            var queue = await client.GetFromJsonAsync<JsonElement>($"/api/v1/admin/listings?page={page}&pageSize=50");
            Assert.True(queue.GetProperty("total").GetInt32() >= 2);
            var items = queue.GetProperty("items").EnumerateArray().ToList();
            Assert.NotEmpty(items);
            found = items.Cast<JsonElement?>().SingleOrDefault(item => item!.Value.GetProperty("id").GetGuid() == approvedId);
        }
        var firstItem = found.Value;
        Assert.Equal("Listing Owner Test", firstItem.GetProperty("ownerName").GetString());
        Assert.Equal(ownerEmail, firstItem.GetProperty("ownerEmail").GetString());
        var imageUrl = Assert.Single(firstItem.GetProperty("images").EnumerateArray()).GetString();
        Assert.Equal(imageBytes, await client.GetByteArrayAsync(imageUrl!));

        await Csrf(client);
        var approve = await client.PostAsJsonAsync($"/api/v1/admin/listings/{approvedId}/approve", new { });
        Assert.Equal(HttpStatusCode.OK, approve.StatusCode);
        await Csrf(client);
        var reject = await client.PostAsJsonAsync($"/api/v1/admin/listings/{rejectedId}/reject",
            new { reasonCode = "image_quality" });
        Assert.Equal(HttpStatusCode.OK, reject.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(
            $"/api/v1/catalog/listings/{approvedId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(
            $"/api/v1/catalog/listings/{rejectedId}")).StatusCode);
        using var auditScope = factory.Services.CreateScope();
        var events = await factory.CatalogDb(auditScope).ListingReviewEvents
            .Where(item => item.ListingId == approvedId || item.ListingId == rejectedId)
            .OrderBy(item => item.Decision).ToListAsync();
        Assert.Collection(events,
            item => { Assert.Equal("Approved", item.Decision); Assert.Equal(adminId, item.AdminUserId); },
            item => { Assert.Equal("Rejected", item.Decision); Assert.Equal("image_quality", item.ReasonCode); Assert.Equal(adminId, item.AdminUserId); });
        using (var cleanupScope = factory.Services.CreateScope())
        {
            var db = factory.CatalogDb(cleanupScope);
            await db.Listings.Where(item => item.Id == approvedId || item.Id == rejectedId).ExecuteDeleteAsync();
        }
        using (var cleanupScope = factory.Services.CreateScope())
        {
            var users = cleanupScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var owner = await users.FindByIdAsync(ownerId.ToString());
            if (owner is not null) Assert.True((await users.DeleteAsync(owner)).Succeeded);
        }
        await factory.DeleteAdmin(email);
    }

    [Fact]
    public async Task Catalog_lists_only_approved_listings_and_supports_filters_and_details()
    {
        await using var factory = new Factory();
        using var client = factory.Client();
        var approvedId = Guid.NewGuid();
        var pendingId = Guid.NewGuid();
        var testPrice = Random.Shared.Next(1_000_000, 9_999_999) / 100m;
        using (var scope = factory.Services.CreateScope())
        {
            var db = factory.CatalogDb(scope);
            db.Listings.AddRange(
                new VehicleListing
                {
                    Id = approvedId, ModelId = 1, Year = 2022, City = "baku",
                    DailyPrice = testPrice, Transmission = "automatic", FuelType = "hybrid",
                    Seats = 5, DescriptionAz = "Test təsviri", DescriptionEn = "Test description",
                    Status = "Approved", CreatedAtUtc = DateTimeOffset.UtcNow
                },
                new VehicleListing
                {
                    Id = pendingId, ModelId = 1, Year = 2024, City = "baku",
                    DailyPrice = testPrice - 20, Transmission = "automatic", FuelType = "petrol",
                    Seats = 5, DescriptionAz = "Pending", DescriptionEn = "Pending",
                    Status = "Pending", CreatedAtUtc = DateTimeOffset.UtcNow
                });
            await db.SaveChangesAsync();
        }

        var priceQuery = testPrice.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var response = await client.GetAsync($"/api/v1/catalog/listings?locale=en&city=baku&make=toyota&minPrice={priceQuery}&maxPrice={priceQuery}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, page.GetProperty("total").GetInt32());
        var item = Assert.Single(page.GetProperty("items").EnumerateArray());
        Assert.Equal("Toyota", item.GetProperty("make").GetString());
        Assert.Equal("Camry", item.GetProperty("model").GetString());
        Assert.Equal(approvedId, item.GetProperty("id").GetGuid());

        var detail = await client.GetFromJsonAsync<JsonElement>($"/api/v1/catalog/listings/{approvedId}?locale=en");
        Assert.Equal("Test description", detail.GetProperty("description").GetString());
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/v1/catalog/listings/{pendingId}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/v1/catalog/listings?minPrice=500&maxPrice=100")).StatusCode);
    }

    [Fact]
    public async Task Catalog_provides_localized_vehicle_make_and_model_options()
    {
        await using var factory = new Factory();
        using var client = factory.Client();
        var makes = await client.GetFromJsonAsync<JsonElement>("/api/v1/catalog/makes?locale=az");
        Assert.Equal(75, makes.GetArrayLength());
        Assert.Contains(makes.EnumerateArray(), make =>
            make.GetProperty("slug").GetString() == "toyota" &&
            make.GetProperty("models").EnumerateArray().Any(model =>
                model.GetProperty("slug").GetString() == "camry"));
        Assert.Contains(makes.EnumerateArray(), make =>
            make.GetProperty("slug").GetString() == "changan" &&
            make.GetProperty("models").EnumerateArray().Any(model =>
                model.GetProperty("slug").GetString() == "cs75-plus"));
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/v1/catalog/makes?locale=fr")).StatusCode);
    }

    [Fact]
    public async Task Verified_owner_can_submit_a_private_pending_listing_with_up_to_five_images()
    {
        await using var factory = new Factory();
        using var client = factory.Client();
        var email = Email();
        await Csrf(client);
        Assert.Equal(HttpStatusCode.Accepted,
            (await client.PostAsJsonAsync("/api/v1/auth/register", Request(email, "owner"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.PostAsJsonAsync("/api/v1/auth/email/verify",
                Verification(await factory.Message(email)))).StatusCode);
        await Csrf(client);
        Assert.Equal(HttpStatusCode.OK,
            (await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = Password })).StatusCode);
        var session = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/session");
        Assert.False(session.GetProperty("canCreateListing").GetBoolean());
        Assert.Equal("Pending", session.GetProperty("accountApprovalStatus").GetString());
        using var ownerScope = factory.Services.CreateScope();
        var userId = await factory.Db(ownerScope).Users.Where(user => user.Email == email)
            .Select(user => user.Id).SingleAsync();

        await Csrf(client);
        using (var pendingContent = new MultipartFormDataContent())
        {
            var pendingResponse = await client.PostAsync("/api/v1/owner/listings", pendingContent);
            Assert.Equal(HttpStatusCode.Forbidden, pendingResponse.StatusCode);
            Assert.Equal("owner_approval_required", (await pendingResponse.Content.ReadFromJsonAsync<JsonElement>())
                .GetProperty("code").GetString());
        }

        var adminEmail = Email();
        var totpSecret = await factory.CreateAdmin(adminEmail);
        using var adminClient = factory.Client();
        await Csrf(adminClient);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.PostAsJsonAsync("/api/v1/auth/login",
            new { email = adminEmail, password = Password, totpCode = Totp(totpSecret) })).StatusCode);
        var pendingAccounts = await adminClient.GetFromJsonAsync<JsonElement>(
            "/api/v1/admin/accounts?page=1&pageSize=50");
        Assert.Contains(pendingAccounts.GetProperty("items").EnumerateArray(), account =>
            account.GetProperty("userId").GetGuid() == userId &&
            account.GetProperty("accountType").GetString() == "owner");
        await Csrf(adminClient);
        var approved = await adminClient.PostAsJsonAsync($"/api/v1/admin/accounts/{userId}/approve", new { });
        Assert.Equal(HttpStatusCode.OK, approved.StatusCode);
        Assert.Equal("Approved", (await approved.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("status").GetString());
        session = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/session");
        Assert.True(session.GetProperty("canCreateListing").GetBoolean());
        Assert.Equal("Approved", session.GetProperty("accountApprovalStatus").GetString());

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("1"), "modelId");
        content.Add(new StringContent("2022"), "year");
        content.Add(new StringContent("baku"), "city");
        content.Add(new StringContent("80.00"), "dailyPrice");
        content.Add(new StringContent("automatic"), "transmission");
        content.Add(new StringContent("hybrid"), "fuelType");
        content.Add(new StringContent("5"), "seats");
        content.Add(new StringContent("Test avtomobil"), "descriptionAz");
        var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jWQAAAABJRU5ErkJggg==");
        var largePng = new byte[6 * 1024 * 1024];
        png.CopyTo(largePng, 0);
        for (var index = 0; index < 3; index++)
        {
            var photo = new ByteArrayContent(index == 0 ? largePng : png);
            photo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            content.Add(photo, "images", $"vehicle-{index}.png");
        }
        var response = await client.PostAsync("/api/v1/owner/listings", content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Pending", result.GetProperty("status").GetString());
        Assert.Equal(3, result.GetProperty("imageCount").GetInt32());
        var listingId = result.GetProperty("id").GetGuid();

        using var scope = factory.Services.CreateScope();
        var db = factory.CatalogDb(scope);
        var listing = await db.Listings.Include(item => item.Images).SingleAsync(item => item.Id == listingId);
        Assert.Equal("Pending", listing.Status);
        Assert.Equal(3, listing.Images.Count);
        // Data appended after the PNG end marker is stripped before storage, like any metadata.
        Assert.All(listing.Images, image => Assert.True(image.Length < 100));
        Assert.All(listing.Images, image => Assert.StartsWith($"{listingId:N}/", image.ObjectKey));
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/v1/catalog/listings/{listingId}/images/{listing.Images[0].Id}")).StatusCode);
        using (var cleanupScope = factory.Services.CreateScope())
        {
            var identityDb = factory.Db(cleanupScope);
            await factory.CatalogDb(cleanupScope).Listings.Where(item => item.Id == listingId).ExecuteDeleteAsync();
            await identityDb.OwnerProfiles.Where(profile => profile.UserId == userId).ExecuteDeleteAsync();
        }
        await factory.DeleteAdmin(adminEmail);
    }

    [Fact]
    public async Task Verified_admin_can_review_company_accounts_and_prevent_duplicate_approved_voen()
    {
        await using var factory = new Factory();
        using var companyClient = factory.Client();
        var companyEmail = Email();
        await Csrf(companyClient);
        Assert.Equal(HttpStatusCode.Accepted,
            (await companyClient.PostAsJsonAsync("/api/v1/auth/register", Request(companyEmail, "company"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await companyClient.PostAsJsonAsync("/api/v1/auth/email/verify",
                Verification(await factory.Message(companyEmail)))).StatusCode);
        Guid companyUserId;
        using (var scope = factory.Services.CreateScope())
            companyUserId = await factory.Db(scope).Users.Where(user => user.Email == companyEmail)
                .Select(user => user.Id).SingleAsync();

        var adminEmail = Email();
        var secret = await factory.CreateAdmin(adminEmail);
        using var adminClient = factory.Client();
        await Csrf(adminClient);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.PostAsJsonAsync("/api/v1/auth/login",
            new { email = adminEmail, password = Password, totpCode = Totp(secret) })).StatusCode);
        Guid adminId;
        using (var scope = factory.Services.CreateScope())
            adminId = await factory.Db(scope).Users.Where(user => user.Email == adminEmail)
                .Select(user => user.Id).SingleAsync();
        Guid existingCompanyUserId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = factory.Db(scope);
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var existingCompany = new ApplicationUser
            {
                UserName = Email(), EmailConfirmed = true, DisplayName = "Existing company",
                Locale = "az", AccountStatus = "Active", CreatedAtUtc = DateTimeOffset.UtcNow
            };
            existingCompany.Email = existingCompany.UserName;
            Assert.True((await users.CreateAsync(existingCompany, Password)).Succeeded);
            existingCompanyUserId = existingCompany.Id;
            var approvedProfile = new CompanyProfile
            {
                UserId = existingCompany.Id, LegalName = "Existing approved company",
                Voen = "1234567890", ContactPhone = "+994501234567",
                SubmittedAtUtc = DateTimeOffset.UtcNow
            };
            db.CompanyProfiles.Add(approvedProfile);
            await db.SaveChangesAsync();
            approvedProfile.Review(ApprovalStatus.Approved, adminId, null, DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
        }
        var queue = await adminClient.GetFromJsonAsync<JsonElement>(
            "/api/v1/admin/accounts?page=1&pageSize=50");
        Assert.Contains(queue.GetProperty("items").EnumerateArray(), account =>
            account.GetProperty("userId").GetGuid() == companyUserId &&
            account.GetProperty("accountType").GetString() == "company" &&
            account.GetProperty("legalName").GetString() == "Example Test Company" &&
            account.GetProperty("voen").GetString() == "1234567890");

        await Csrf(adminClient);
        var duplicateVoen = await adminClient.PostAsJsonAsync(
            $"/api/v1/admin/accounts/{companyUserId}/approve", new { });
        Assert.Equal(HttpStatusCode.Conflict, duplicateVoen.StatusCode);
        Assert.Equal("company_voen_already_approved",
            (await duplicateVoen.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        await Csrf(adminClient);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await adminClient.PostAsJsonAsync($"/api/v1/admin/accounts/{companyUserId}/reject",
                new { reasonCode = "unsupported_reason" })).StatusCode);
        await Csrf(adminClient);
        var rejected = await adminClient.PostAsJsonAsync($"/api/v1/admin/accounts/{companyUserId}/reject",
            new { reasonCode = "incomplete_information" });
        Assert.Equal(HttpStatusCode.OK, rejected.StatusCode);
        Assert.Equal("Rejected", (await rejected.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("status").GetString());

        await Csrf(companyClient);
        Assert.Equal(HttpStatusCode.OK, (await companyClient.PostAsJsonAsync("/api/v1/auth/login",
            new { email = companyEmail, password = Password })).StatusCode);
        var session = await companyClient.GetFromJsonAsync<JsonElement>("/api/v1/auth/session");
        Assert.False(session.GetProperty("canCreateListing").GetBoolean());
        Assert.Equal("Rejected", session.GetProperty("accountApprovalStatus").GetString());
        Assert.Equal("incomplete_information", session.GetProperty("accountApprovalReason").GetString());
        using (var cleanupScope = factory.Services.CreateScope())
            await factory.Db(cleanupScope).CompanyProfiles
                .Where(profile => profile.UserId == companyUserId || profile.UserId == existingCompanyUserId)
                .ExecuteDeleteAsync();
        await factory.DeleteAdmin(adminEmail);
    }

    [Fact]
    public async Task Testing_environment_refuses_main_database_before_opening_connection()
    {
        await using var factory = new Factory { MisconfiguredDatabase = true };
        using var client = factory.Client(); await Csrf(client);
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", Request(Email()));
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain("Password", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Local_mail_has_plain_text_link_without_json_escaping_or_token_changes()
    {
        await using var factory = new Factory(); using var client = factory.Client(); await Csrf(client);
        var email = Email();
        Assert.Equal(HttpStatusCode.Accepted, (await client.PostAsJsonAsync("/api/v1/auth/register", Request(email))).StatusCode);
        var message = await factory.Message(email);
        var id = message.GetProperty("id").GetGuid();
        var plain = await File.ReadAllTextAsync(Path.Combine(factory.Mailbox, id + ".txt"));
        var url = message.GetProperty("verificationUrl").GetString()!;
        Assert.True(plain.Split(Environment.NewLine).Contains(url), "Text email must preserve the exact URL.");
        Assert.False(plain.Contains(@"\u0026", StringComparison.Ordinal), "Plain text must not contain JSON escapes.");
        var raw = await File.ReadAllTextAsync(Path.Combine(factory.Mailbox, id + ".json"));
        Assert.True(raw.Contains(@"\u0026", StringComparison.Ordinal), "Default safe JSON encoding must remain enabled.");
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/email/verify", Verification(message))).StatusCode);
    }

    [Fact]
    public async Task Verification_survives_disposing_and_recreating_the_application_host()
    {
        var email = Email();
        object payload;
        await using (var first = new Factory())
        {
            using var client = first.Client(); await Csrf(client);
            Assert.Equal(HttpStatusCode.Accepted, (await client.PostAsJsonAsync("/api/v1/auth/register", Request(email))).StatusCode);
            payload = Verification(await first.Message(email));
        }
        await using var restarted = new Factory();
        using var afterRestart = restarted.Client(); await Csrf(afterRestart);
        Assert.Equal(HttpStatusCode.OK, (await afterRestart.PostAsJsonAsync("/api/v1/auth/email/verify", payload)).StatusCode);
    }

    [Fact]
    public async Task Resend_rejection_reports_a_bounded_remaining_interval()
    {
        await using var factory = new Factory(); using var client = factory.Client(); await Csrf(client);
        var email = Email();
        for (var i = 0; i < 3; i++)
            Assert.Equal(HttpStatusCode.Accepted, (await client.PostAsJsonAsync("/api/v1/auth/email/resend", new { email })).StatusCode);
        var rejected = await client.PostAsJsonAsync("/api/v1/auth/email/resend", new { email });
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.InRange(rejected.Headers.RetryAfter!.Delta!.Value.TotalSeconds, 1, 900);
    }

    private sealed class FaultFactory : Factory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services => services.AddDbContext<IdentityDbContext>((_, options) => options.AddInterceptors(new FailProfile())));
        }
    }
    private sealed class FailProfile : Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor
    {
        public override ValueTask<Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int>> SavingChangesAsync(
            Microsoft.EntityFrameworkCore.Diagnostics.DbContextEventData eventData,
            Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int> result, CancellationToken ct = default)
        {
            if (eventData.Context!.ChangeTracker.Entries<OwnerProfile>().Any(e => e.State == EntityState.Added))
                throw new InvalidOperationException("Intentional test persistence fault.");
            return ValueTask.FromResult(result);
        }
    }
}
