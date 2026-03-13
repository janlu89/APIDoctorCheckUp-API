namespace APIDoctorCheckUp.Domain.Enums;

/// <summary>
/// Describes what condition caused an incident to be opened for an endpoint.
/// Stored on the Incident entity so the dashboard can display a meaningful reason.
/// </summary>
public enum IncidentTrigger
{
    /// <summary>
    /// The endpoint failed to respond successfully for the configured number of consecutive checks.
    /// </summary>
    ConsecutiveFailures = 1,

    /// <summary>
    /// The endpoint is responding but response time has exceeded the critical threshold.
    /// </summary>
    SlowResponse = 2
}
