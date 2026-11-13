using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TurboRent.BuildingBlocks.Http;
using TurboRent.Modules.Catalog;
using TurboRent.Modules.Catalog.Features.AdminReview;
using TurboRent.Modules.Catalog.Features.Bookings;
using TurboRent.Modules.Catalog.Features.BrowseListings;
using TurboRent.Modules.Catalog.Features.OwnerListings;
using TurboRent.Modules.Identity;
using TurboRent.Modules.Identity.Features.AdminApprovals;
using TurboRent.Modules.Identity.Features.AdminBootstrap;
using TurboRent.Modules.Identity.Infrastructure.Security;
using TurboRent.Modules.System;
using TurboRent.Modules.System.Features.GetSystemStatus;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddSystemModule(builder.Configuration);
builder.Services.AddIdentityRegistration(builder.Configuration, builder.Environment);
builder.Services.AddCatalog(builder.Configuration, builder.Environment);
ConfigureForwardedHeaders(builder.Services, builder.Configuration);
ConfigureRateLimits(builder.Services, builder.Configuration);

var app = builder.Build();
// Evaluated after Build so settings supplied by the host (and test hosts) are visible.
var accountsEnabled = IdentityModule.AccountsEnabled(app.Environment, app.Configuration);
// Must run first so rate limits, HTTPS checks and logs see the real client, not the proxy.
app.UseForwardedHeaders();
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing")) app.UseHsts();
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers.XContentTypeOptions = "nosniff";
    headers.XFrameOptions = "DENY";
    // API responses are data, never documents; image endpoints set their own sandbox policy.
    context.Response.OnStarting(() =>
    {
        if (!headers.ContainsKey("Content-Security-Policy"))
            headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
        if (!headers.ContainsKey("Referrer-Policy"))
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        return Task.CompletedTask;
    });
    await next(context);
});
app.UseRateLimiter();

if (accountsEnabled)
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/api/v1/auth"))
        {
            context.Response.Headers.CacheControl = "no-store";
            var size = context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
            if (size is { IsReadOnly: false }) size.MaxRequestBodySize = 16384;
            if (context.Request.ContentLength > 16384)
            {
                await ApiProblem.Error(413, "request_too_large").ExecuteAsync(context);
                return;
            }
        }
        await next(context);
    });
    app.UseStatusCodePages(async status =>
    {
        if (status.HttpContext.Request.Path.StartsWithSegments("/api/v1/auth"))
            await ApiProblem.Error(status.HttpContext.Response.StatusCode, "invalid_request")
                .ExecuteAsync(status.HttpContext);
    });
    app.MapIdentityRegistration();
    app.MapAdminAccountApprovals();
    app.MapOwnerListings();
    app.MapBookings();
    app.MapLocalAdminBootstrap();
    app.MapLocalAdminTotpRecovery();
    app.MapAdminReview();
}
else
{
    app.Logger.LogWarning("Account features are disabled: configure Email:Smtp:Host to enable sign-up and sign-in.");
}
app.MapCatalog();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapGet("/api/v1/health/live", () => Results.Ok(SystemStatus.Healthy()))
    .WithName("Liveness").WithTags("Health");
app.MapHealthChecks("/api/v1/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status == HealthStatus.Healthy ? "healthy" : "unhealthy",
            checks = report.Entries.ToDictionary(entry => entry.Key,
                entry => entry.Value.Status == HealthStatus.Healthy ? "healthy" : "unhealthy")
        });
    }
});
app.Run();

// Only proxies inside ReverseProxy:TrustedNetworks (CIDR list) may set X-Forwarded-For/-Proto.
// Without this every request appears to come from the nginx container and shares one rate limit.
static void ConfigureForwardedHeaders(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        var networks = configuration["ReverseProxy:TrustedNetworks"]?
            .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        foreach (var network in networks) options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
    });
}

static void ConfigureRateLimits(IServiceCollection services, IConfiguration configuration)
{
    services.AddRateLimiter(options =>
    {
        void PerIp(string policy, string key, int defaultLimit) => options.AddPolicy(policy, context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = configuration.GetValue(key, defaultLimit),
                    Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true
                }));
        PerIp(RateLimitPolicies.Identity, "Identity:IpPermitLimit", 20);
        PerIp(RateLimitPolicies.Writes, "RateLimits:WritesPerMinute", 30);
        PerIp(RateLimitPolicies.Dashboard, "RateLimits:DashboardPerMinute", 300);
        PerIp(RateLimitPolicies.Contact, "RateLimits:ContactPerMinute", 10);
        options.OnRejected = async (context, ct) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            context.HttpContext.Response.Headers.RetryAfter = RetryAfter.Seconds(context.Lease, TimeSpan.FromMinutes(1));
            await context.HttpContext.Response.WriteAsJsonAsync(new { code = "rate_limited" }, ct);
        };
    });
}

public partial class Program;
