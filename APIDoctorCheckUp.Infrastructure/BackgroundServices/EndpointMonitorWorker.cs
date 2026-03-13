using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace APIDoctorCheckUp.Infrastructure.BackgroundServices;

/// <summary>
/// Manages the continuous health check loop for a single monitored endpoint.
/// Each instance runs independently on its own Task, allowing endpoints to be
/// checked on different intervals without blocking each other.
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
        _endpointId   = endpointId;
        _scopeFactory = scopeFactory;
        _logger       = logger;
        _cts          = new CancellationTokenSource();
    }

    public void Start(CancellationToken applicationStopping)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            _cts.Token, applicationStopping);

        _workerTask = Task.Run(() => RunAsync(linkedCts.Token), linkedCts.Token);

        _logger.LogInformation(
            "Monitor worker started for endpoint {EndpointId}", _endpointId);
    }

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
        // Stagger startup to avoid all workers hitting the database simultaneously
        var startupDelay = TimeSpan.FromSeconds(Random.Shared.Next(1, 15));
        await Task.Delay(startupDelay, ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var endpointRepo    = scope.ServiceProvider.GetRequiredService<IEndpointRepository>();
                var checkResultRepo = scope.ServiceProvider.GetRequiredService<ICheckResultRepository>();
                var checker         = scope.ServiceProvider.GetRequiredService<IEndpointChecker>();
                var alertEvaluator  = scope.ServiceProvider.GetRequiredService<IAlertEvaluator>();

                var endpoint = await endpointRepo.GetByIdAsync(_endpointId, ct);

                if (endpoint is null || !endpoint.IsActive)
                {
                    _logger.LogInformation(
                        "Endpoint {EndpointId} is no longer active — stopping worker", _endpointId);
                    return;
                }

                // Execute the HTTP check and persist the result
                var result = await checker.CheckAsync(endpoint, ct);
                await checkResultRepo.AddAsync(result, ct);

                // Evaluate thresholds and manage incident lifecycle
                var newStatus = await alertEvaluator.EvaluateAsync(endpoint, result, ct);

                if (endpoint.CurrentStatus != newStatus)
                {
                    endpoint.CurrentStatus = newStatus;
                    await endpointRepo.UpdateAsync(endpoint, ct);

                    _logger.LogInformation(
                        "Endpoint {EndpointName} status changed: {OldStatus} ? {NewStatus}",
                        endpoint.Name, endpoint.CurrentStatus, newStatus);
                }

                await Task.Delay(TimeSpan.FromSeconds(endpoint.CheckIntervalSeconds), ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled error in monitor worker for endpoint {EndpointId}. " +
                    "Waiting 30 seconds before retrying.", _endpointId);

                try { await Task.Delay(TimeSpan.FromSeconds(30), ct); }
                catch (OperationCanceledException) { return; }
            }
        }
    }
}
