namespace APIDoctorCheckUp.Domain.Enums;

/// <summary>
/// Represents the current operational status of a monitored endpoint,
/// derived from the most recent check results and alert threshold evaluation.
/// </summary>
public enum EndpointStatus
{
    /// <summary>
    /// Status has not yet been determined — no checks have been run since startup.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The endpoint is responding within acceptable thresholds.
    /// </summary>
    Up = 1,

    /// <summary>
    /// The endpoint is responding but response time exceeds the warning threshold.
    /// </summary>
    Degraded = 2,

    /// <summary>
    /// The endpoint has failed the consecutive failure threshold and is considered down.
    /// </summary>
    Down = 3
}
