using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

namespace APIDoctorCheckUp.Api.Extensions;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Registers the health check services.
    /// </summary>
    public static IServiceCollection AddHealthChecksConfig(
        this IServiceCollection services)
    {
        services.AddHealthChecks();
        // Day 7: .AddDbContextCheck<AppDbContext>("database")
        return services;
    }

    /// <summary>
    /// Maps /health to return a structured JSON response instead of the
    /// default plain-text "Healthy" string. UptimeRobot will ping this
    /// endpoint every 5 minutes to prevent the Render.com instance from sleeping.
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
                    timestamp = DateTime.UtcNow
                });

                await context.Response.WriteAsync(result);
            }
        });

        return app;
    }
}
