using APIDoctorCheckUp.Application.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace APIDoctorCheckUp.Infrastructure.SignalR;

/// <summary>
/// SignalR hub at /hubs/monitor. Lives in Infrastructure alongside the
/// broadcaster so both can share the hub type without circular project references.
/// Intentionally thin — handles connection lifecycle and group membership only.
/// All broadcasting is performed by MonitoringBroadcaster via IHubContext.
/// </summary>
public class MonitorHub : Hub<IMonitorHubClient>
{
    private readonly ILogger<MonitorHub> _logger;

    public MonitorHub(ILogger<MonitorHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called by clients to subscribe to all monitoring updates.
    /// All connected clients receive all broadcasts — the dashboard
    /// displays the same public data to every visitor.
    /// </summary>
    public async Task JoinDashboard()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
        _logger.LogDebug("Client {ConnectionId} joined dashboard", Context.ConnectionId);
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogDebug("Client connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogDebug("Client disconnected: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
