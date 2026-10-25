using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace TurboRent.IntegrationTests;

public class HealthTests
{
    private static WebApplicationFactory<Program> CreateFactory(string connection) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseEnvironment("Development").ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database"] = connection
                })));

    [Fact]
    public async Task Liveness_does_not_depend_on_database()
    {
        await using var factory = CreateFactory("Host=127.0.0.1;Port=1;Database=missing;Username=test;Password=test;Timeout=1");
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("healthy", body.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Readiness_returns_503_when_database_is_unavailable()
    {
        await using var factory = CreateFactory("Host=127.0.0.1;Port=1;Database=missing;Username=test;Password=test;Timeout=1");
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/health/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.DoesNotContain("Password", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Readiness_connects_to_real_postgres()
    {
        var connection = Environment.GetEnvironmentVariable("TURBORENT_TEST_DATABASE")
            ?? throw new InvalidOperationException("Set TURBORENT_TEST_DATABASE to the test PostgreSQL connection string.");
        await using var factory = CreateFactory(connection);
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/health/ready")).StatusCode);
    }

    [Fact]
    public async Task Development_exposes_openapi()
    {
        await using var factory = CreateFactory("Host=127.0.0.1;Port=1;Database=test;Username=test;Password=test");
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/openapi/v1.json")).StatusCode);
    }
}
