using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Domain.Entities;
using APIDoctorCheckUp.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace APIDoctorCheckUp.Infrastructure.BackgroundServices;

/// <summary>
/// Manages the continuous health check loop for a single monitored endpoint.
/// Each instance runs independently on its own Task, allowing endpoints to be
/// checked on different intervals without blocking each other.
///
/// Scoped services (DbContext, repositories) are resolved fresh on each check
/// iteration via IServiceScopeFactory, which is the correct pattern for
/// accessing scoped services from a long-running background component.
/// </summary>
public sealed class EndpointMonitorWorker
{
    private readonly int _endpointId;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EndpointMonitorWorker> _logger;
    private readonly CancellationTokenSource _cts;

    private Task? _workerTask;

    public int EndpointId => _endpointId;

    public EndpointMonitorWorker(
        int endpointId,
        IServiceScopeFactory scopeFactory,
        ILogger<EndpointMonitorWorker> logger)
    {
        _endpointId = endpointId;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _cts = new CancellationTokenSource();
    }

    /// <summary>
    /// Starts the check loop on a background Task.
    /// The loop continues until Stop() is called or the application shuts down.
    /// </summary>
    public void Start(CancellationToken applicationStopping)
    {
        // Link the application shutdown token with this worker's own token.
        // Either signal will stop the loop — application shutdown or explicit Stop() call.
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            _cts.Token, applicationStopping);

        _workerTask = Task.Run(() => RunAsync(linkedCts.Token), linkedCts.Token);

        _logger.LogInformation(
            "Monitor worker started for endpoint {EndpointId}", _endpointId);
    }

    /// <summary>
    /// Signals the worker to stop and waits for the current check to complete.
    /// Safe to call even if the worker has already stopped.
    /// </summary>
    public async Task StopAsync()
    {
        await _cts.CancelAsync();

        if (_workerTask is not null)
        {
            try { await _workerTask; }
            catch (OperationCanceledException) { /* expected on cancellation */ }
        }

        _logger.LogInformation(
            "Monitor worker stopped for endpoint {EndpointId}", _endpointId);
    }

    private async Task RunAsync(CancellationToken ct)
    {
        // Stagger worker startup slightly to avoid all workers hammering the
        // database simultaneously on application boot.
        var startupDelay = TimeSpan.FromSeconds(Random.Shared.Next(1, 15));
        await Task.Delay(startupDelay, ct);

        while (!ct.IsCancellationRequested)
        {
            MonitoredEndpoint? endpoint = null;

            try
            {
                // Resolve a fresh scope for each check — this is the correct
                // pattern for using scoped services (DbContext) from a long-
                // running background component.
                using var scope = _scopeFactory.CreateScope();

                var endpointRepo   = scope.ServiceProvider.GetRequiredService<IEndpointRepository>();
                var checkResultRepo = scope.ServiceProvider.GetRequiredService<ICheckResultRepository>();
                var checker        = scope.ServiceProvider.GetRequiredService<IEndpointChecker>();

                endpoint = await endpointRepo.GetByIdAsync(_endpointId, ct);

                // Endpoint was deleted or deactivated — stop the worker
                if (endpoint is null || !endpoint.IsActive)
                {
                    _logger.LogInformation(
                        "Endpoint {EndpointId} is no longer active — stopping worker", _endpointId);
                    return;
                }

                // Execute the HTTP check and persist the result
                var result = await checker.CheckAsync(endpoint, ct);
                await checkResultRepo.AddAsync(result, ct);

                // Update the endpoint's CurrentStatus based on this result.
                // Day 5 replaces this simple logic with full threshold evaluation.
                var newStatus = result.IsSuccess ? EndpointStatus.Up : EndpointStatus.Down;

                if (endpoint.CurrentStatus != newStatus)
                {
                    endpoint.CurrentStatus = newStatus;
                    await endpointRepo.UpdateAsync(endpoint, ct);

                    _logger.LogInformation(
                        "Endpoint {EndpointName} status changed to {Status}",
                        endpoint.Name, newStatus);
                }

                // Wait the configured interval before the next check
                await Task.Delay(TimeSpan.FromSeconds(endpoint.CheckIntervalSeconds), ct);
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown — exit the loop cleanly
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled error in monitor worker for endpoint {EndpointId}. " +
                    "Waiting 30 seconds before retrying.", _endpointId);

                // Back off briefly rather than hammering a broken dependency
                try { await Task.Delay(TimeSpan.FromSeconds(30), ct); }
                catch (OperationCanceledException) { return; }
            }
        }
    }
}
