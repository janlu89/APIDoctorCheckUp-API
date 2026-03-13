using APIDoctorCheckUp.Domain.Entities;
using APIDoctorCheckUp.Domain.Enums;

namespace APIDoctorCheckUp.Application.Interfaces;

/// <summary>
/// Evaluates a completed check result against the endpoint's configured alert
/// thresholds and returns the new status the endpoint should be assigned.
/// Also manages the incident lifecycle — opening incidents on failure and
/// closing them on recovery.
/// </summary>
public interface IAlertEvaluator
{
    /// <summary>
    /// Evaluates the result of a single health check and returns the status
    /// that the endpoint should be transitioned to. Persists any incident
    /// changes as a side effect.
    /// </summary>
    Task<EndpointStatus> EvaluateAsync(
        MonitoredEndpoint endpoint,
        CheckResult result,
        CancellationToken ct = default);
}
