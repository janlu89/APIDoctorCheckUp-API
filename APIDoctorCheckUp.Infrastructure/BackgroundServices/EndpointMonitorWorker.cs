using APIDoctorCheckUp.Application.DTOs;
using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace APIDoctorCheckUp.Infrastructure.BackgroundServices;

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
            catch (OperationCanceledException) { }
        }

        _logger.LogInformation(
            "Monitor worker stopped for endpoint {EndpointId}", _endpointId);
    }

    private async Task RunAsync(CancellationToken ct)
    {
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
                var broadcaster     = scope.ServiceProvider.GetRequiredService<IMonitoringBroadcaster>();

                var endpoint = await endpointRepo.GetByIdAsync(_endpointId, ct);

                if (endpoint is null || !endpoint.IsActive)
                {
                    _logger.LogInformation(
                        "Endpoint {EndpointId} is no longer active — stopping worker", _endpointId);
                    return;
                }

                var result     = await checker.CheckAsync(endpoint, ct);
                await checkResultRepo.AddAsync(result, ct);

                var previousStatus = endpoint.CurrentStatus;
                var newStatus      = await alertEvaluator.EvaluateAsync(endpoint, result, ct);

                if (endpoint.CurrentStatus != newStatus)
                {
                    endpoint.CurrentStatus = newStatus;
                    await endpointRepo.UpdateAsync(endpoint, ct);

                    // Broadcast the status change to all connected clients
                    await broadcaster.BroadcastStatusChangedAsync(new StatusChangedBroadcast(
                        EndpointId:     endpoint.Id,
                        EndpointName:   endpoint.Name,
                        PreviousStatus: previousStatus,
                        NewStatus:      newStatus,
                        ChangedAt:      DateTime.UtcNow), ct);
                }

                // Broadcast the check result regardless of whether status changed
                await broadcaster.BroadcastCheckResultAsync(new CheckResultBroadcast(
                    EndpointId:     endpoint.Id,
                    EndpointName:   endpoint.Name,
                    CheckedAt:      result.CheckedAt,
                    StatusCode:     result.StatusCode,
                    ResponseTimeMs: result.ResponseTimeMs,
                    IsSuccess:      result.IsSuccess,
                    ErrorMessage:   result.ErrorMessage,
                    CurrentStatus:  newStatus), ct);

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
