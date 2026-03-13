using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Infrastructure.BackgroundServices;
using APIDoctorCheckUp.Infrastructure.Http;

namespace APIDoctorCheckUp.Api.Extensions;

public static class MonitoringExtensions
{
    /// <summary>
    /// Registers the monitoring engine: the HTTP client, the endpoint checker,
    /// and the background orchestrator that manages per-endpoint workers.
    ///
    /// The orchestrator is registered as a singleton first, then referenced by
    /// AddHostedService and IMonitoringOrchestrator. This ensures the same
    /// instance is used everywhere — the hosted service lifetime and any
    /// controller that injects IMonitoringOrchestrator both get the same object.
    /// </summary>
    public static IServiceCollection AddMonitoringEngine(
        this IServiceCollection services)
    {
        // Named HttpClient with a 20-second timeout per check request.
        // Using IHttpClientFactory means connections are pooled and recycled
        // correctly, avoiding the socket exhaustion issues of newing HttpClient directly.
        services.AddHttpClient("MonitoringClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.Add(
                "User-Agent", "APIDoctorCheckUp-Monitor/1.0");
        });

        services.AddScoped<IEndpointChecker, EndpointChecker>();

        // Register the orchestrator as a singleton so it can be resolved both
        // as IHostedService (by the host) and as IMonitoringOrchestrator (by controllers)
        services.AddSingleton<MonitoringOrchestrator>();
        services.AddHostedService(sp => sp.GetRequiredService<MonitoringOrchestrator>());
        services.AddSingleton<IMonitoringOrchestrator>(
            sp => sp.GetRequiredService<MonitoringOrchestrator>());

        return services;
    }
}
