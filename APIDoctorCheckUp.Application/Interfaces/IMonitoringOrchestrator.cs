namespace APIDoctorCheckUp.Application.Interfaces;

/// <summary>
/// Manages the lifecycle of per-endpoint monitor workers.
/// Exposed as an interface in Application so the API controllers can signal
/// the orchestrator when endpoints are created or deleted — without taking
/// a direct dependency on the Infrastructure implementation.
/// </summary>
public interface IMonitoringOrchestrator
{
    /// <summary>
    /// Starts a background worker for the specified endpoint.
    /// Called by the API after a new endpoint is successfully created.
    /// </summary>
    Task StartEndpointAsync(int endpointId);

    /// <summary>
    /// Stops and removes the background worker for the specified endpoint.
    /// Called by the API before an endpoint is deleted.
    /// </summary>
    Task StopEndpointAsync(int endpointId);
}
