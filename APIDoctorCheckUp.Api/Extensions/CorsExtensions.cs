namespace APIDoctorCheckUp.Api.Extensions;

public static class CorsExtensions
{
    public const string FrontendPolicy = "Frontend";

    /// <summary>
    /// Registers the CORS policy that allows the Angular frontend to communicate
    /// with this API. Allowed origins are read from configuration so that the
    /// local dev origin (localhost:4200) and the production Vercel origin can
    /// be supplied via environment variables without touching code.
    /// </summary>
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? ["http://localhost:4200"];

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendPolicy, policy =>
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials());
        });

        return services;
    }

    /// <summary>
    /// Adds the CORS middleware to the request pipeline.
    /// Must be called before UseAuthentication and UseAuthorization so that
    /// browser preflight OPTIONS requests are handled before any auth checks run.
    /// </summary>
    public static WebApplication UseCorsPolicy(this WebApplication app)
    {
        app.UseCors(FrontendPolicy);
        return app;
    }
}
