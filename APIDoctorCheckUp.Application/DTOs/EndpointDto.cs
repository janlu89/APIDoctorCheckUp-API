using APIDoctorCheckUp.Domain.Enums;

namespace APIDoctorCheckUp.Application.DTOs;

public record EndpointDto(
    int Id,
    string Name,
    string Url,
    int ExpectedStatusCode,
    int CheckIntervalSeconds,
    bool IsActive,
    DateTime CreatedAt,
    EndpointStatus CurrentStatus,
    AlertThresholdDto? AlertThreshold
);

public record AlertThresholdDto(
    int Id,
    int ResponseTimeWarningMs,
    int ResponseTimeCriticalMs,
    int ConsecutiveFailuresDown
);
