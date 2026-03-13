using APIDoctorCheckUp.Domain.Entities;
using APIDoctorCheckUp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace APIDoctorCheckUp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MonitoredEndpoint> MonitoredEndpoints => Set<MonitoredEndpoint>();
    public DbSet<CheckResult> CheckResults => Set<CheckResult>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<AlertThreshold> AlertThresholds => Set<AlertThreshold>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // CreatedAt must be a hardcoded static value in seed data.
        // EF Core compares the model snapshot to the current model on every build —
        // any dynamic value (DateTime.UtcNow, Guid.NewGuid()) causes a mismatch
        // that is flagged as a pending migration and blocks database update.
        var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var endpoints = new[]
        {
            new MonitoredEndpoint { Id = 1, Name = "HTTPBin GET",              Url = "https://httpbin.org/get",                     CheckIntervalSeconds = 60,  CreatedAt = seededAt },
            new MonitoredEndpoint { Id = 2, Name = "JSONPlaceholder Post",     Url = "https://jsonplaceholder.typicode.com/posts/1", CheckIntervalSeconds = 60,  CreatedAt = seededAt },
            new MonitoredEndpoint { Id = 3, Name = "GitHub API",               Url = "https://api.github.com",                      CheckIntervalSeconds = 120, CreatedAt = seededAt },
            new MonitoredEndpoint { Id = 4, Name = "Cat Fact",                 Url = "https://catfact.ninja/fact",                  CheckIntervalSeconds = 120, CreatedAt = seededAt },
            new MonitoredEndpoint { Id = 5, Name = "Snippet Vault API",        Url = "https://snippetvault.onrender.com/health",     CheckIntervalSeconds = 300, CreatedAt = seededAt },
            new MonitoredEndpoint { Id = 6, Name = "JSON to C# Generator API", Url = "https://jsontocsharp.onrender.com/health",     CheckIntervalSeconds = 300, CreatedAt = seededAt },
        };

        modelBuilder.Entity<MonitoredEndpoint>().HasData(endpoints);

        var thresholds = endpoints.Select((e, i) => new AlertThreshold
        {
            Id                      = i + 1,
            EndpointId              = e.Id,
            ResponseTimeWarningMs   = 1000,
            ResponseTimeCriticalMs  = 3000,
            ConsecutiveFailuresDown = 3
        }).ToArray();

        modelBuilder.Entity<AlertThreshold>().HasData(thresholds);
    }
}
