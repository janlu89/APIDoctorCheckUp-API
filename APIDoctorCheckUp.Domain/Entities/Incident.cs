using APIDoctorCheckUp.Domain.Enums;

namespace APIDoctorCheckUp.Domain.Entities;

/// <summary>
/// Represents a period during which a monitored endpoint was non-operational
/// or degraded. An incident is opened by the alert engine when a threshold is
/// breached and closed automatically when the endpoint recovers.
/// </summary>
public class Incident
{
    public int Id { get; set; }
    public int EndpointId { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Null while the incident is still active. Set to the UTC timestamp
    /// of the first successful check after the failure period.
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    public IncidentTrigger TriggerReason { get; set; }

    /// <summary>
    /// Computed convenience property. An incident with no ResolvedAt is still open.
    /// Not persisted to the database.
    /// </summary>
    public bool IsOpen => ResolvedAt is null;

    /// <summary>
    /// Duration of the incident. Null while still open.
    /// Not persisted to the database.
    /// </summary>
    public TimeSpan? Duration => ResolvedAt.HasValue
        ? ResolvedAt.Value - StartedAt
        : null;

    // -- Navigation property ------------------------------------------------
    public MonitoredEndpoint Endpoint { get; set; } = null!;
}
