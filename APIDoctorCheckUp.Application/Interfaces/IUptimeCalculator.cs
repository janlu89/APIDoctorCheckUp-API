namespace APIDoctorCheckUp.Application.Interfaces;

/// <summary>
/// Calculates uptime percentage for a monitored endpoint over a given time window.
/// Defined in Application so the calculation logic can be tested independently
/// of any persistence or HTTP concerns.
/// </summary>
public interface IUptimeCalculator
{
    /// <summary>
    /// Returns uptime as a percentage (0.0 to 100.0) for the given endpoint
    /// over the specified number of hours ending at the current UTC time.
    /// </summary>
    Task<double> CalculateAsync(int endpointId, int hours, CancellationToken ct = default);
}
