namespace APIDoctorCheckUp.Domain.Entities;

/// <summary>
/// Configurable thresholds that determine when the monitoring engine changes
/// an endpoint's status from Up to Degraded or Down, and opens an incident.
/// Each MonitoredEndpoint has at most one AlertThreshold (one-to-one relationship).
/// </summary>
public class AlertThreshold
{
    public int Id { get; set; }
    public int EndpointId { get; set; }

    /// <summary>
    /// Response times exceeding this value trigger a Degraded status.
    /// Default is 1000ms (1 second).
    /// </summary>
    public int ResponseTimeWarningMs { get; set; } = 1000;

    /// <summary>
    /// Response times exceeding this value trigger a Down status and open an incident.
    /// Default is 3000ms (3 seconds).
    /// </summary>
    public int ResponseTimeCriticalMs { get; set; } = 3000;

    /// <summary>
    /// Number of consecutive failed checks required before the endpoint is
    /// marked as Down and an incident is opened. Default is 3 consecutive failures.
    /// </summary>
    public int ConsecutiveFailuresDown { get; set; } = 3;

    // -- Navigation property ------------------------------------------------
    public MonitoredEndpoint Endpoint { get; set; } = null!;
}
