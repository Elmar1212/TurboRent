using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TurboRent.Modules.System;
using TurboRent.Modules.System.Features.GetSystemStatus;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddSystemModule(builder.Configuration);

var app = builder.Build();
app.UseExceptionHandler();
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

public partial class Program;
