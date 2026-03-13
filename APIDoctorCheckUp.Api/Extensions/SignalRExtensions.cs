using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Infrastructure.SignalR;

namespace APIDoctorCheckUp.Api.Extensions;

public static class SignalRExtensions
{
    public static IServiceCollection AddSignalRServices(
        this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<IMonitoringBroadcaster, MonitoringBroadcaster>();
        return services;
    }

    public static WebApplication UseSignalRHubs(this WebApplication app)
    {
        app.MapHub<MonitorHub>("/hubs/monitor");
        return app;
    }
}
