using APIDoctorCheckUp.Application.DTOs;

namespace APIDoctorCheckUp.Application.Interfaces;

/// <summary>
/// Pushes real-time monitoring events to all connected SignalR clients.
/// Defined in Application so the Infrastructure worker can broadcast without
/// taking a direct dependency on SignalR or the Api project.
/// </summary>
public interface IMonitoringBroadcaster
{
    /// <summary>Broadcasts the result of a completed health check.</summary>
    Task BroadcastCheckResultAsync(
        CheckResultBroadcast payload,
        CancellationToken ct = default);

    /// <summary>Broadcasts an endpoint status transition.</summary>
    Task BroadcastStatusChangedAsync(
        StatusChangedBroadcast payload,
        CancellationToken ct = default);
}
