using APIDoctorCheckUp.Domain.Enums;

namespace APIDoctorCheckUp.Application.DTOs;

/// <summary>
/// Payload broadcast to all connected clients after every health check completes.
/// Contains enough information for the dashboard to update without a separate API call.
/// </summary>
public record CheckResultBroadcast(
    int EndpointId,
    string EndpointName,
    DateTime CheckedAt,
    int? StatusCode,
    long ResponseTimeMs,
    bool IsSuccess,
    string? ErrorMessage,
    EndpointStatus CurrentStatus
);

/// <summary>
/// Payload broadcast when an endpoint transitions between status values.
/// The dashboard uses this to update status indicators and trigger alerts.
/// </summary>
public record StatusChangedBroadcast(
    int EndpointId,
    string EndpointName,
    EndpointStatus PreviousStatus,
    EndpointStatus NewStatus,
    DateTime ChangedAt
);
