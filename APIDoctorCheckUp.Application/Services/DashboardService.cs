using APIDoctorCheckUp.Application.DTOs;
using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Domain.Enums;

namespace APIDoctorCheckUp.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IEndpointRepository _endpoints;
    private readonly ICheckResultRepository _checkResults;
    private readonly IUptimeCalculator _uptime;

    public DashboardService(
        IEndpointRepository endpoints,
        ICheckResultRepository checkResults,
        IUptimeCalculator uptime)
    {
        _endpoints = endpoints;
        _checkResults = checkResults;
        _uptime = uptime;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var endpoints = (await _endpoints.GetAllAsync(ct)).ToList();

        var summaries = await Task.WhenAll(endpoints.Select(async e =>
        {
            var latest  = await _checkResults.GetLatestByEndpointIdAsync(e.Id, ct);
            var uptime  = await _uptime.CalculateAsync(e.Id, 24, ct);

            return new EndpointSummaryDto(
                Id:                 e.Id,
                Name:               e.Name,
                Url:                e.Url,
                CurrentStatus:      e.CurrentStatus,
                LastResponseTimeMs: latest?.ResponseTimeMs,
                LastCheckedAt:      latest?.CheckedAt,
                UptimeLast24Hours:  uptime);
        }));

        return new DashboardSummaryDto(
            TotalEndpoints: endpoints.Count,
            UpCount:        endpoints.Count(e => e.CurrentStatus == EndpointStatus.Up),
            DegradedCount:  endpoints.Count(e => e.CurrentStatus == EndpointStatus.Degraded),
            DownCount:      endpoints.Count(e => e.CurrentStatus == EndpointStatus.Down),
            UnknownCount:   endpoints.Count(e => e.CurrentStatus == EndpointStatus.Unknown),
            Endpoints:      summaries);
    }
}
