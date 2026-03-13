using APIDoctorCheckUp.Domain.Entities;

namespace APIDoctorCheckUp.Application.Interfaces;

/// <summary>
/// Executes a single HTTP health check against a monitored endpoint and
/// returns an unpersisted CheckResult. The caller is responsible for saving
/// the result to the database.
/// </summary>
public interface IEndpointChecker
{
    Task<CheckResult> CheckAsync(MonitoredEndpoint endpoint, CancellationToken ct = default);
}
