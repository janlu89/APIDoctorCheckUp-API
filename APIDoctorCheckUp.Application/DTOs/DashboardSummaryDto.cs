using APIDoctorCheckUp.Domain.Enums;

namespace APIDoctorCheckUp.Application.DTOs;

public record DashboardSummaryDto(
    int TotalEndpoints,
    int UpCount,
    int DegradedCount,
    int DownCount,
    int UnknownCount,
    IEnumerable<EndpointSummaryDto> Endpoints
);

public record EndpointSummaryDto(
    int Id,
    string Name,
    string Url,
    EndpointStatus CurrentStatus,
    long? LastResponseTimeMs,
    DateTime? LastCheckedAt,
    double UptimeLast24Hours
);
