using APIDoctorCheckUp.Application.DTOs;
using APIDoctorCheckUp.Application.Hubs;
using APIDoctorCheckUp.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace APIDoctorCheckUp.Infrastructure.SignalR;

public class MonitoringBroadcaster : IMonitoringBroadcaster
{
    private readonly IHubContext<MonitorHub, IMonitorHubClient> _hubContext;
    private readonly ILogger<MonitoringBroadcaster> _logger;

    public MonitoringBroadcaster(
        IHubContext<MonitorHub, IMonitorHubClient> hubContext,
        ILogger<MonitoringBroadcaster> logger)
    {
        _hubContext = hubContext;
        _logger     = logger;
    }

    public async Task BroadcastCheckResultAsync(
        CheckResultBroadcast payload,
        CancellationToken ct = default)
    {
        await _hubContext.Clients.Group("dashboard").OnCheckResult(payload);
        _logger.LogDebug(
            "Broadcast OnCheckResult for endpoint {EndpointId}", payload.EndpointId);
    }

    public async Task BroadcastStatusChangedAsync(
        StatusChangedBroadcast payload,
        CancellationToken ct = default)
    {
        await _hubContext.Clients.Group("dashboard").OnStatusChanged(payload);
        _logger.LogInformation(
            "Broadcast OnStatusChanged for {EndpointName}: {Previous} ? {New}",
            payload.EndpointName, payload.PreviousStatus, payload.NewStatus);
    }
}
