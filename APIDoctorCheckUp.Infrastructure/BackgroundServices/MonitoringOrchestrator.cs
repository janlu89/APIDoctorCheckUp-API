using APIDoctorCheckUp.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace APIDoctorCheckUp.Infrastructure.BackgroundServices;

/// <summary>
/// Hosted service that manages the lifecycle of all per-endpoint monitor workers.
/// Starts on application boot, spawns one worker per active endpoint, and
/// exposes IMonitoringOrchestrator so the API controllers can dynamically
/// start and stop workers as endpoints are created and deleted.
/// </summary>
public sealed class MonitoringOrchestrator : BackgroundService, IMonitoringOrchestrator
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonitoringOrchestrator> _logger;

    // Thread-safe dictionary of active workers keyed by endpoint ID.
    // ConcurrentDictionary is used because API requests (controller actions)
    // and the background thread both access this collection.
    private readonly ConcurrentDictionary<int, EndpointMonitorWorker> _workers = new();

    public MonitoringOrchestrator(
        IServiceScopeFactory scopeFactory,
        ILogger<MonitoringOrchestrator> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Entry point called by the ASP.NET Core host when the application starts.
    /// Loads all active endpoints from the database and starts a worker for each.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Monitoring orchestrator starting...");

        // Load all active endpoints and start their workers
        using (var scope = _scopeFactory.CreateScope())
        {
            var endpointRepo = scope.ServiceProvider.GetRequiredService<IEndpointRepository>();
            var endpoints    = await endpointRepo.GetAllAsync(stoppingToken);
            var active       = endpoints.Where(e => e.IsActive).ToList();

            _logger.LogInformation(
                "Starting workers for {Count} active endpoints", active.Count);

            foreach (var endpoint in active)
                StartWorker(endpoint.Id, stoppingToken);
        }

        // Keep ExecuteAsync alive until the application shuts down.
        // The workers run independently on their own Tasks — this method
        // just needs to stay alive so the hosted service is not disposed.
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    /// <summary>
    /// Starts a new worker for the given endpoint ID.
    /// Called internally on startup and externally via IMonitoringOrchestrator
    /// when the API creates a new endpoint.
    /// </summary>
    public Task StartEndpointAsync(int endpointId)
    {
        // StoppingToken is not directly accessible outside ExecuteAsync,
        // so we use CancellationToken.None here. The worker links its own
        // CancellationTokenSource for explicit stop signals.
        StartWorker(endpointId, CancellationToken.None);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops and removes the worker for the given endpoint ID.
    /// Called by the API before an endpoint is deleted.
    /// </summary>
    public async Task StopEndpointAsync(int endpointId)
    {
        if (_workers.TryRemove(endpointId, out var worker))
        {
            await worker.StopAsync();
            _logger.LogInformation(
                "Worker removed for endpoint {EndpointId}", endpointId);
        }
    }

    /// <summary>
    /// Called by the host on application shutdown. Stops all active workers cleanly.
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Monitoring orchestrator stopping — shutting down {Count} workers",
            _workers.Count);

        var stopTasks = _workers.Values.Select(w => w.StopAsync());
        await Task.WhenAll(stopTasks);

        _workers.Clear();
        await base.StopAsync(cancellationToken);
    }

    private void StartWorker(int endpointId, CancellationToken stoppingToken)
    {
        if (_workers.ContainsKey(endpointId))
        {
            _logger.LogWarning(
                "Worker for endpoint {EndpointId} is already running — skipping", endpointId);
            return;
        }

        var worker = new EndpointMonitorWorker(
            endpointId,
            _scopeFactory,
            _logger as ILogger<EndpointMonitorWorker>
                ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<EndpointMonitorWorker>.Instance);

        if (_workers.TryAdd(endpointId, worker))
            worker.Start(stoppingToken);
    }
}
