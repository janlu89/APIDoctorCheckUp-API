using APIDoctorCheckUp.Domain.Enums;

namespace APIDoctorCheckUp.Domain.Entities;

/// <summary>
/// Represents a REST API endpoint that is being actively monitored.
/// Each active endpoint has a corresponding background worker that checks
/// it on the configured interval.
/// </summary>
public class MonitoredEndpoint
{
    public int Id { get; set; }

    /// <summary>
    /// Human-readable display name shown on the dashboard.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The full URL that the background worker will send an HTTP GET request to.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP status code the monitoring engine considers a successful response.
    /// Defaults to 200 but can be configured for endpoints that legitimately return
    /// other codes (e.g. 204, 301).
    /// </summary>
    public int ExpectedStatusCode { get; set; } = 200;

    /// <summary>
    /// How frequently the background worker checks this endpoint, in seconds.
    /// Minimum enforced value is 30 seconds to avoid hammering external APIs.
    /// </summary>
    public int CheckIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// When false the background worker skips this endpoint entirely.
    /// Allows temporary suspension without deleting history.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Derived from the most recent check results. Recalculated and pushed
    /// via SignalR on every check completion.
    /// </summary>
    public EndpointStatus CurrentStatus { get; set; } = EndpointStatus.Unknown;

    // -- Navigation properties ----------------------------------------------
    public ICollection<CheckResult> CheckResults { get; set; } = [];
    public ICollection<Incident> Incidents { get; set; } = [];
    public AlertThreshold? AlertThreshold { get; set; }
}
