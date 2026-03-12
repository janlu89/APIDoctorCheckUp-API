using APIDoctorCheckUp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APIDoctorCheckUp.Api.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        // SQLite for local development, PostgreSQL (Npgsql) wired on Day 13
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        return services;
    }
}
