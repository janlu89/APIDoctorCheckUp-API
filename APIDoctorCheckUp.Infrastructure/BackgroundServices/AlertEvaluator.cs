using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Domain.Entities;
using APIDoctorCheckUp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace APIDoctorCheckUp.Infrastructure.BackgroundServices;

public class AlertEvaluator : IAlertEvaluator
{
    private readonly ICheckResultRepository _checkResults;
    private readonly IIncidentRepository _incidents;
    private readonly ILogger<AlertEvaluator> _logger;

    public AlertEvaluator(
        ICheckResultRepository checkResults,
        IIncidentRepository incidents,
        ILogger<AlertEvaluator> logger)
    {
        _checkResults = checkResults;
        _incidents    = incidents;
        _logger       = logger;
    }

    public async Task<EndpointStatus> EvaluateAsync(
        MonitoredEndpoint endpoint,
        CheckResult result,
        CancellationToken ct = default)
    {
        var threshold = endpoint.AlertThreshold;

        // When no threshold is configured, fall back to simple success/failure.
        if (threshold is null)
            return result.IsSuccess ? EndpointStatus.Up : EndpointStatus.Down;

        var newStatus = await DetermineStatusAsync(endpoint, result, threshold, ct);
        await HandleIncidentLifecycleAsync(endpoint, newStatus, ct);

        return newStatus;
    }

    // -- Status Determination --------------------------------------------------

    private async Task<EndpointStatus> DetermineStatusAsync(
        MonitoredEndpoint endpoint,
        CheckResult result,
        AlertThreshold threshold,
        CancellationToken ct)
    {
        // A failed check (wrong status code, timeout, or network error) always
        // contributes to the consecutive failure count. Evaluate that first.
        if (!result.IsSuccess)
        {
            var consecutiveFailures = await _checkResults
                .GetConsecutiveFailureCountAsync(endpoint.Id, ct);

            if (consecutiveFailures >= threshold.ConsecutiveFailuresDown)
            {
                _logger.LogWarning(
                    "Endpoint {EndpointName} is DOWN — {Count} consecutive failures",
                    endpoint.Name, consecutiveFailures);

                return EndpointStatus.Down;
            }

            // Failed but not yet at the Down threshold — keep previous status
            // or return Unknown if this is the very first check.
            return endpoint.CurrentStatus == EndpointStatus.Unknown
                ? EndpointStatus.Unknown
                : endpoint.CurrentStatus;
        }

        // The check succeeded — now evaluate response time thresholds.
        if (result.ResponseTimeMs >= threshold.ResponseTimeCriticalMs)
        {
            _logger.LogWarning(
                "Endpoint {EndpointName} is DOWN — response time {Ms}ms exceeds critical threshold {Threshold}ms",
                endpoint.Name, result.ResponseTimeMs, threshold.ResponseTimeCriticalMs);

            return EndpointStatus.Down;
        }

        if (result.ResponseTimeMs >= threshold.ResponseTimeWarningMs)
        {
            _logger.LogInformation(
                "Endpoint {EndpointName} is DEGRADED — response time {Ms}ms exceeds warning threshold {Threshold}ms",
                endpoint.Name, result.ResponseTimeMs, threshold.ResponseTimeWarningMs);

            return EndpointStatus.Degraded;
        }

        return EndpointStatus.Up;
    }

    // -- Incident Lifecycle ----------------------------------------------------

    private async Task HandleIncidentLifecycleAsync(
        MonitoredEndpoint endpoint,
        EndpointStatus newStatus,
        CancellationToken ct)
    {
        var wasDown  = endpoint.CurrentStatus == EndpointStatus.Down;
        var isNowDown = newStatus == EndpointStatus.Down;

        if (!wasDown && isNowDown)
        {
            await OpenIncidentAsync(endpoint, ct);
        }
        else if (wasDown && !isNowDown)
        {
            await CloseIncidentAsync(endpoint, ct);
        }
    }

    private async Task OpenIncidentAsync(MonitoredEndpoint endpoint, CancellationToken ct)
    {
        // Guard against duplicate open incidents in case of a race condition
        var existing = await _incidents.GetOpenIncidentAsync(endpoint.Id, ct);
        if (existing is not null) return;

        var incident = new Incident
        {
            EndpointId    = endpoint.Id,
            StartedAt     = DateTime.UtcNow,
            TriggerReason = IncidentTrigger.ConsecutiveFailures
        };

        await _incidents.AddAsync(incident, ct);

        _logger.LogWarning(
            "Incident opened for endpoint {EndpointName} (Id: {EndpointId})",
            endpoint.Name, endpoint.Id);
    }

    private async Task CloseIncidentAsync(MonitoredEndpoint endpoint, CancellationToken ct)
    {
        var incident = await _incidents.GetOpenIncidentAsync(endpoint.Id, ct);
        if (incident is null) return;

        incident.ResolvedAt = DateTime.UtcNow;
        await _incidents.UpdateAsync(incident, ct);

        _logger.LogInformation(
            "Incident resolved for endpoint {EndpointName} — duration {Duration}",
            endpoint.Name, incident.Duration);
    }
}
