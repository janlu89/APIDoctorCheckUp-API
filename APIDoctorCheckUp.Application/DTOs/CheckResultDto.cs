namespace APIDoctorCheckUp.Application.DTOs;

public record CheckResultDto(
    int Id,
    int EndpointId,
    DateTime CheckedAt,
    int? StatusCode,
    long ResponseTimeMs,
    bool IsSuccess,
    string? ErrorMessage
);
