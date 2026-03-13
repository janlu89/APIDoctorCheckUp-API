using APIDoctorCheckUp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

namespace APIDoctorCheckUp.Api.Extensions;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Registers the health check services.
    /// AddDbContextCheck opens a real DB connection on every probe so that
    /// a broken or missing database is surfaced as Unhealthy, not just a
    /// dead process. This is what UptimeRobot actually needs to monitor.
    /// </summary>
    public static IServiceCollection AddHealthChecksConfig(
        this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database");

        return services;
    }

    /// <summary>
    /// Maps /health to return structured JSON instead of plain-text "Healthy".
    /// The response includes per-probe results so we can see database status
    /// separately from overall process status.
    /// </summary>
    public static WebApplication UseHealthChecksEndpoint(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    status    = report.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    checks    = report.Entries.Select(e => new
                    {
                        name        = e.Key,
                        status      = e.Value.Status.ToString(),
                        description = e.Value.Description
                    })
                });

                await context.Response.WriteAsync(result);
            }
        });

        return app;
    }
}
