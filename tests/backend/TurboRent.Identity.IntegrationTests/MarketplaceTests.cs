using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TurboRent.Modules.Catalog.Infrastructure.Storage;
using TurboRent.Modules.Identity.Domain;

namespace TurboRent.Identity.IntegrationTests;

/// <summary>Session refresh, password reset, owner listing management and bookings.</summary>
public partial class RegistrationTests
{
    private sealed class MovableTime : TimeProvider
    {
        public TimeSpan Offset { get; set; }
        public override DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow + Offset;
    }

    private static async Task<HttpClient> SignedIn(Factory factory, string email, string? totp = null)
    {
        var client = factory.Client();
        await Csrf(client);
        var response = await client.PostAsJsonAsync("/api/v1/auth/login",
            totp is null ? new { email, password = Password } : (object)new { email, password = Password, totpCode = totp });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await Csrf(client);
        return client;
    }

    private static async Task<Guid> CreateUser(Factory factory, string email, string name = "Marketplace Test")
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = email, Email = email, EmailConfirmed = true, DisplayName = name,
            Locale = "en", AccountStatus = "Active", CreatedAtUtc = DateTimeOffset.UtcNow
        };
        Assert.True((await users.CreateAsync(user, Password)).Succeeded);
        Assert.True((await users.AddToRoleAsync(user, IdentityRoles.Customer)).Succeeded);
        return user.Id;
    }

    private static async Task<Guid> CreateApprovedOwner(Factory factory, string email, Guid reviewerId)
    {
        var userId = await CreateUser(factory, email, "Owner Test");
        using var scope = factory.Services.CreateScope();
        var db = factory.Db(scope);
        var profile = new OwnerProfile { UserId = userId, ContactPhone = "+994501112233", SubmittedAtUtc = DateTimeOffset.UtcNow };
        profile.Review(ApprovalStatus.Approved, reviewerId, null, DateTimeOffset.UtcNow);
        db.OwnerProfiles.Add(profile);
        await db.SaveChangesAsync();
        return userId;
    }

    private static async Task<JsonElement> MailOfKind(Factory factory, string email, string kind)
    {
        foreach (var file in Directory.GetFiles(factory.Mailbox, "*.json"))
        {
            var message = JsonDocument.Parse(await File.ReadAllTextAsync(file)).RootElement.Clone();
            if (message.GetProperty("email").GetString() == email &&
                message.TryGetProperty("kind", out var value) && value.GetString() == kind) return message;
        }
        throw new InvalidOperationException($"Expected {kind} message missing.");
    }

    /// <summary>A JPEG whose EXIF block contains a fake GPS marker.</summary>
    private static byte[] JpegWithExif()
    {
        var exif = Encoding.ASCII.GetBytes("Exif\0\0GPS-SECRET-40.4093,49.8671");
        var bytes = new List<byte> { 0xff, 0xd8 };
        bytes.AddRange([0xff, 0xe0, 0x00, 0x10, 0x4a, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00]);
        bytes.AddRange([0xff, 0xe1, (byte)((exif.Length + 2) >> 8), (byte)((exif.Length + 2) & 0xff)]);
        bytes.AddRange(exif);
        bytes.AddRange([0xff, 0xda, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3f, 0x00, 0x12, 0x34, 0x56, 0xff, 0xd9]);
        return bytes.ToArray();
    }

    [Fact]
    public async Task Activity_extends_the_session_idle_timeout()
    {
        var time = new MovableTime();
        await using var factory = new Factory { Time = time };
        var email = Email();
        await CreateUser(factory, email);
        using var client = await SignedIn(factory, email);

        time.Offset = TimeSpan.FromMinutes(25);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/auth/session")).StatusCode);
        // 50 minutes after sign-in is past the original 30-minute idle limit, but the
        // request at minute 25 extended it.
        time.Offset = TimeSpan.FromMinutes(50);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/auth/session")).StatusCode);
        // A real idle period still ends the session.
        time.Offset = TimeSpan.FromMinutes(90);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/auth/session")).StatusCode);
    }

    [Fact]
    public async Task Password_reset_changes_the_password_and_ends_existing_sessions()
    {
        await using var factory = new Factory();
        var email = Email();
        await CreateUser(factory, email);
        using var oldSession = await SignedIn(factory, email);
        using var client = factory.Client();
        await Csrf(client);

        Assert.Equal(HttpStatusCode.Accepted,
            (await client.PostAsJsonAsync("/api/v1/auth/password/forgot", new { email })).StatusCode);
        // Unknown addresses get the same answer, so accounts cannot be discovered.
        Assert.Equal(HttpStatusCode.Accepted,
            (await client.PostAsJsonAsync("/api/v1/auth/password/forgot", new { email = Email() })).StatusCode);

        var message = await MailOfKind(factory, email, "password_reset");
        var fragment = QueryHelpers.ParseQuery(new Uri(message.GetProperty("resetUrl").GetString()!).Fragment.TrimStart('#'));
        var newPassword = "A brand new long passphrase 2026";
        var reset = new { userId = fragment["userId"].ToString(), token = fragment["token"].ToString(), password = newPassword };
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/password/reset", reset)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/auth/password/reset", reset)).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await oldSession.GetAsync("/api/v1/auth/session")).StatusCode);
        await Csrf(client);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = Password })).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = newPassword })).StatusCode);
    }

    [Fact]
    public async Task Owner_manages_listing_and_renter_books_confirmed_dates()
    {
        await using var factory = new Factory();
        var adminEmail = Email();
        var totpSecret = await factory.CreateAdmin(adminEmail);
        Guid adminId;
        using (var scope = factory.Services.CreateScope())
            adminId = await factory.Db(scope).Users.Where(user => user.Email == adminEmail).Select(user => user.Id).SingleAsync();
        var ownerEmail = Email();
        var ownerId = await CreateApprovedOwner(factory, ownerEmail, adminId);
        var renterEmail = Email();
        await CreateUser(factory, renterEmail, "Renter Test");
        var secondRenterEmail = Email();
        await CreateUser(factory, secondRenterEmail, "Second Renter");
        Guid listingId = Guid.Empty;
        try
        {
            using var owner = await SignedIn(factory, ownerEmail);
            using (var content = new MultipartFormDataContent())
            {
                foreach (var (name, value) in new[] { ("modelId", "1"), ("year", "2023"), ("city", "baku"),
                    ("dailyPrice", "75.50"), ("transmission", "automatic"), ("fuelType", "petrol"), ("seats", "5"),
                    ("descriptionAz", "Təmiz avtomobil") })
                    content.Add(new StringContent(value), name);
                var photo = new ByteArrayContent(JpegWithExif());
                photo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                content.Add(photo, "images", "car.jpg");
                var created = await owner.PostAsync("/api/v1/owner/listings", content);
                Assert.Equal(HttpStatusCode.Created, created.StatusCode);
                listingId = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
            }

            // The stored photo has no EXIF block, so no location data is published.
            using (var scope = factory.Services.CreateScope())
            {
                var key = await factory.CatalogDb(scope).ListingImages.Where(image => image.ListingId == listingId)
                    .Select(image => image.ObjectKey).SingleAsync();
                await using var stored = await scope.ServiceProvider.GetRequiredService<IVehicleImageStorage>()
                    .OpenReadAsync(key, CancellationToken.None);
                using var reader = new MemoryStream();
                await stored!.CopyToAsync(reader);
                Assert.DoesNotContain("GPS-SECRET", Encoding.ASCII.GetString(reader.ToArray()));
            }

            var mine = await owner.GetFromJsonAsync<JsonElement>("/api/v1/owner/listings?locale=en");
            var listed = Assert.Single(mine.GetProperty("items").EnumerateArray());
            Assert.Equal("Pending", listed.GetProperty("status").GetString());

            using var admin = await SignedIn(factory, adminEmail, Totp(totpSecret));
            var rejected = await admin.PostAsJsonAsync($"/api/v1/admin/listings/{listingId}/reject", new { reasonCode = "image_quality" });
            Assert.Equal(HttpStatusCode.OK, rejected.StatusCode);
            mine = await owner.GetFromJsonAsync<JsonElement>("/api/v1/owner/listings?locale=en");
            Assert.Equal("image_quality", mine.GetProperty("items")[0].GetProperty("rejectionReason").GetString());
            await MailOfKind(factory, ownerEmail, "ListingRejected");

            // Editing a rejected listing sends it back to review.
            var edit = new
            {
                modelId = 1, year = 2023, city = "baku", dailyPrice = 80m, transmission = "automatic",
                fuelType = "petrol", seats = 5, descriptionAz = "Yenilənmiş təsvir", descriptionEn = "Updated"
            };
            var edited = await owner.PutAsJsonAsync($"/api/v1/owner/listings/{listingId}", edit);
            Assert.Equal(HttpStatusCode.OK, edited.StatusCode);
            Assert.Equal("Pending", (await edited.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("status").GetString());
            Assert.Equal(HttpStatusCode.OK, (await admin.PostAsJsonAsync($"/api/v1/admin/listings/{listingId}/approve", new { })).StatusCode);

            using var anonymous = factory.Client();
            var detail = await anonymous.GetFromJsonAsync<JsonElement>($"/api/v1/catalog/listings/{listingId}?locale=en");
            Assert.Equal("Owner Test", detail.GetProperty("ownerName").GetString());
            Assert.False(detail.TryGetProperty("ownerEmail", out _));
            var contact = await anonymous.GetFromJsonAsync<JsonElement>($"/api/v1/catalog/listings/{listingId}/contact");
            Assert.Equal("+994501112233", contact.GetProperty("phone").GetString());

            var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
            using var renter = await SignedIn(factory, renterEmail);
            var request = await renter.PostAsJsonAsync("/api/v1/bookings",
                new { listingId, startDate = start, endDate = start.AddDays(2), message = "Airport pickup" });
            Assert.Equal(HttpStatusCode.Created, request.StatusCode);
            var booking = await request.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(240m, booking.GetProperty("totalPrice").GetDecimal());
            await MailOfKind(factory, ownerEmail, "BookingRequested");

            using var secondRenter = await SignedIn(factory, secondRenterEmail);
            var competing = await secondRenter.PostAsJsonAsync("/api/v1/bookings",
                new { listingId, startDate = start.AddDays(1), endDate = start.AddDays(4) });
            Assert.Equal(HttpStatusCode.Created, competing.StatusCode);
            var competingId = (await competing.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

            Assert.Equal(HttpStatusCode.BadRequest, (await owner.PostAsJsonAsync("/api/v1/bookings",
                new { listingId, startDate = start, endDate = start })).StatusCode);

            var incoming = await owner.GetFromJsonAsync<JsonElement>("/api/v1/owner/bookings");
            Assert.Equal(2, incoming.GetProperty("items").GetArrayLength());
            Assert.All(incoming.GetProperty("items").EnumerateArray(), item =>
                Assert.Equal(JsonValueKind.Null, item.GetProperty("contact").GetProperty("email").ValueKind));
            var bookingId = booking.GetProperty("id").GetGuid();
            Assert.Equal(HttpStatusCode.OK, (await owner.PostAsJsonAsync($"/api/v1/owner/bookings/{bookingId}/confirm", new { })).StatusCode);

            // Confirming declines the overlapping request and blocks those days.
            var secondView = await secondRenter.GetFromJsonAsync<JsonElement>("/api/v1/bookings");
            Assert.Equal("Declined", secondView.GetProperty("items").EnumerateArray()
                .Single(item => item.GetProperty("id").GetGuid() == competingId).GetProperty("status").GetString());
            Assert.Equal(HttpStatusCode.Conflict, (await secondRenter.PostAsJsonAsync("/api/v1/bookings",
                new { listingId, startDate = start.AddDays(2), endDate = start.AddDays(3) })).StatusCode);
            var availability = await anonymous.GetFromJsonAsync<JsonElement>($"/api/v1/catalog/listings/{listingId}/availability");
            Assert.Single(availability.GetProperty("booked").EnumerateArray());

            var renterView = await renter.GetFromJsonAsync<JsonElement>("/api/v1/bookings");
            var confirmed = Assert.Single(renterView.GetProperty("items").EnumerateArray());
            Assert.Equal("Confirmed", confirmed.GetProperty("status").GetString());
            Assert.Equal("+994501112233", confirmed.GetProperty("contact").GetProperty("phone").GetString());

            Assert.Equal(HttpStatusCode.Conflict, (await owner.DeleteAsync($"/api/v1/owner/listings/{listingId}")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await owner.PostAsJsonAsync($"/api/v1/owner/listings/{listingId}/archive", new { })).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/v1/catalog/listings/{listingId}")).StatusCode);
        }
        finally
        {
            using var scope = factory.Services.CreateScope();
            var catalog = factory.CatalogDb(scope);
            await catalog.Bookings.Where(item => item.ListingId == listingId).ExecuteDeleteAsync();
            await catalog.Listings.Where(item => item.Id == listingId).ExecuteDeleteAsync();
            await factory.Db(scope).OwnerProfiles.Where(profile => profile.UserId == ownerId).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task Production_with_smtp_maps_account_routes_and_trusts_only_configured_proxies()
    {
        await using var production = new Factory
        {
            EnvironmentName = "Production",
            ExtraSettings = new()
            {
                ["Email:Smtp:Host"] = "smtp.invalid",
                ["Identity:DataProtectionKeysPath"] = Path.Combine(Path.GetTempPath(), "turborent-keys-" + Guid.NewGuid().ToString("N")),
                ["ReverseProxy:TrustedNetworks"] = "127.0.0.0/8"
            }
        };
        using var client = production.Client();
        var csrf = await client.GetAsync("/api/v1/auth/csrf");
        Assert.Equal(HttpStatusCode.OK, csrf.StatusCode);
        Assert.Equal("nosniff", csrf.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", csrf.Headers.GetValues("X-Frame-Options").Single());
    }
}
