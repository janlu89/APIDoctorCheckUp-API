using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Application.Services;
using APIDoctorCheckUp.Infrastructure.Persistence;

namespace APIDoctorCheckUp.Api.Extensions;

public static class ApplicationServicesExtensions
{
    /// <summary>
    /// Registers all Application-layer services and Infrastructure repositories
    /// into the DI container. Controllers and background services receive these
    /// via constructor injection — they never reference concrete types directly.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Repositories — scoped to the HTTP request lifetime
        services.AddScoped<IEndpointRepository, EndpointRepository>();
        services.AddScoped<ICheckResultRepository, CheckResultRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();

        // Application services
        services.AddScoped<IEndpointService, EndpointService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IUptimeCalculator, UptimeCalculator>();

        return services;
    }
}
