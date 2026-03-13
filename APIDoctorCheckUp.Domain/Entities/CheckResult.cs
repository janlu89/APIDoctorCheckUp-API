namespace APIDoctorCheckUp.Domain.Entities;

/// <summary>
/// An immutable record of a single health check execution against a monitored endpoint.
/// CheckResults are append-only — they are never updated after creation.
/// </summary>
public class CheckResult
{
    public int Id { get; set; }
    public int EndpointId { get; set; }

    /// <summary>
    /// UTC timestamp of when the HTTP request was initiated.
    /// </summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The HTTP status code returned. Null if the request timed out or
    /// threw a network-level exception before a response was received.
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Total round-trip time from request initiation to response body read, in milliseconds.
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// True when the status code matches the endpoint's ExpectedStatusCode
    /// and no exception was thrown during the check.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Populated when IsSuccess is false. Contains the exception message
    /// or a description of why the check failed (timeout, DNS failure, etc.).
    /// </summary>
    public string? ErrorMessage { get; set; }

    // -- Navigation property ------------------------------------------------
    public MonitoredEndpoint Endpoint { get; set; } = null!;
}
