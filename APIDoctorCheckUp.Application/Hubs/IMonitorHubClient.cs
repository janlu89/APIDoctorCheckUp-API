using APIDoctorCheckUp.Application.DTOs;

namespace APIDoctorCheckUp.Application.Hubs;

/// <summary>
/// Defines the server-to-client SignalR contract.
/// Lives in Application so both the Api hub and Infrastructure broadcaster
/// can reference it without creating a circular project dependency.
/// </summary>
public interface IMonitorHubClient
{
    Task OnCheckResult(CheckResultBroadcast payload);
    Task OnStatusChanged(StatusChangedBroadcast payload);
}
