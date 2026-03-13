namespace APIDoctorCheckUp.Application.DTOs;

public record EndpointStatsDto(
    int EndpointId,
    double UptimeLast24Hours,
    double UptimeLast7Days,
    double UptimeLast30Days,
    double AverageResponseTimeMs,
    int TotalIncidents,
    int OpenIncidents
);
