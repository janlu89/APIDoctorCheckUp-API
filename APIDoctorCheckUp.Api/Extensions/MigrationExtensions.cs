using APIDoctorCheckUp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APIDoctorCheckUp.Api.Extensions;

public static class MigrationExtensions
{
    /// <summary>
    /// Applies any pending EF Core migrations on startup and creates the database
    /// if it does not already exist. This is idempotent — if the database is
    /// already up to date, this is a no-op.
    ///
    /// This is essential for Docker: the container has no database file on first
    /// start, so we must create and seed it programmatically rather than relying
    /// on a file that was pre-created on the developer's machine.
    ///
    /// The same code works unchanged on Day 13 when we switch to PostgreSQL on
    /// Neon.tech — MigrateAsync applies pending migrations against any provider.
    /// </summary>
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        app.Logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        app.Logger.LogInformation("Database migrations applied successfully.");
    }
}
