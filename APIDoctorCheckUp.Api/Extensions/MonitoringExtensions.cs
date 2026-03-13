using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Infrastructure.BackgroundServices;
using APIDoctorCheckUp.Infrastructure.Http;

namespace APIDoctorCheckUp.Api.Extensions;

public static class MonitoringExtensions
{
    public static IServiceCollection AddMonitoringEngine(
        this IServiceCollection services)
    {
        services.AddHttpClient("MonitoringClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.Add(
                "User-Agent", "APIDoctorCheckUp-Monitor/1.0");
        });

        services.AddScoped<IEndpointChecker, EndpointChecker>();
        services.AddScoped<IAlertEvaluator, AlertEvaluator>();

        // The orchestrator is registered as a singleton and resolved as both
        // IHostedService and IMonitoringOrchestrator so the same instance
        // is shared between the host lifetime and the API controllers.
        services.AddSingleton<MonitoringOrchestrator>();
        services.AddHostedService(sp => sp.GetRequiredService<MonitoringOrchestrator>());
        services.AddSingleton<IMonitoringOrchestrator>(
            sp => sp.GetRequiredService<MonitoringOrchestrator>());

        return services;
    }
}
