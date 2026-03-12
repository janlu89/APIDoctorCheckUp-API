using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace APIDoctorCheckUp.Api.Extensions;

public static class JwtExtensions
{
    /// <summary>
    /// Registers JWT bearer authentication. The signing key, issuer, and audience
    /// are read from configuration so they can be supplied via environment variables
    /// on Render.com without touching code.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var secret   = configuration["JWT:Secret"]
            ?? throw new InvalidOperationException("JWT:Secret is not configured.");
        var issuer   = configuration["JWT:Issuer"]   ?? "APIDoctorCheckUp";
        var audience = configuration["JWT:Audience"] ?? "APIDoctorCheckUp";

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = issuer,
                    ValidAudience            = audience,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                                                 Encoding.UTF8.GetBytes(secret)),
                    ClockSkew                = TimeSpan.Zero // No grace period on expiry
                };
            });

        services.AddAuthorization();
        return services;
    }
}
