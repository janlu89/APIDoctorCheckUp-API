using APIDoctorCheckUp.Application.DTOs;
using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace APIDoctorCheckUp.Application.Services;

public class EndpointService : IEndpointService
{
    private readonly IEndpointRepository _endpoints;
    private readonly ICheckResultRepository _checkResults;
    private readonly IIncidentRepository _incidents;
    private readonly IUptimeCalculator _uptime;
    private readonly ILogger<EndpointService> _logger;

    public EndpointService(
        IEndpointRepository endpoints,
        ICheckResultRepository checkResults,
        IIncidentRepository incidents,
        IUptimeCalculator uptime,
        ILogger<EndpointService> logger)
    {
        _endpoints = endpoints;
        _checkResults = checkResults;
        _incidents = incidents;
        _uptime = uptime;
        _logger = logger;
    }

    public async Task<IEnumerable<EndpointDto>> GetAllAsync(CancellationToken ct = default)
    {
        var endpoints = await _endpoints.GetAllAsync(ct);
        return endpoints.Select(MapToDto);
    }

    public async Task<EndpointDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var endpoint = await _endpoints.GetByIdAsync(id, ct);
        return endpoint is null ? null : MapToDto(endpoint);
    }

    public async Task<EndpointDto> CreateAsync(CreateEndpointDto dto, CancellationToken ct = default)
    {
        var endpoint = new MonitoredEndpoint
        {
            Name                 = dto.Name,
            Url                  = dto.Url,
            ExpectedStatusCode   = dto.ExpectedStatusCode,
            CheckIntervalSeconds = dto.CheckIntervalSeconds,
            CreatedAt            = DateTime.UtcNow,
            AlertThreshold = new AlertThreshold
            {
                ResponseTimeWarningMs   = dto.ResponseTimeWarningMs,
                ResponseTimeCriticalMs  = dto.ResponseTimeCriticalMs,
                ConsecutiveFailuresDown = dto.ConsecutiveFailuresDown
            }
        };

        var created = await _endpoints.AddAsync(endpoint, ct);
        _logger.LogInformation("Created endpoint {EndpointId} — {Name}", created.Id, created.Name);
        return MapToDto(created);
    }

    public async Task<EndpointDto?> UpdateAsync(
        int id,
        UpdateEndpointDto dto,
        CancellationToken ct = default)
    {
        var endpoint = await _endpoints.GetByIdAsync(id, ct);
        if (endpoint is null) return null;

        endpoint.Name                 = dto.Name;
        endpoint.Url                  = dto.Url;
        endpoint.ExpectedStatusCode   = dto.ExpectedStatusCode;
        endpoint.CheckIntervalSeconds = dto.CheckIntervalSeconds;
        endpoint.IsActive             = dto.IsActive;

        if (endpoint.AlertThreshold is not null)
        {
            endpoint.AlertThreshold.ResponseTimeWarningMs   = dto.ResponseTimeWarningMs;
            endpoint.AlertThreshold.ResponseTimeCriticalMs  = dto.ResponseTimeCriticalMs;
            endpoint.AlertThreshold.ConsecutiveFailuresDown = dto.ConsecutiveFailuresDown;
        }

        await _endpoints.UpdateAsync(endpoint, ct);
        _logger.LogInformation("Updated endpoint {EndpointId}", id);
        return MapToDto(endpoint);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var endpoint = await _endpoints.GetByIdAsync(id, ct);
        if (endpoint is null) return false;

        await _endpoints.DeleteAsync(id, ct);
        _logger.LogInformation("Deleted endpoint {EndpointId}", id);
        return true;
    }

    public async Task<IEnumerable<CheckResultDto>> GetChecksAsync(
        int id,
        int limit,
        CancellationToken ct = default)
    {
        var results = await _checkResults.GetByEndpointIdAsync(id, limit, ct);
        return results.Select(r => new CheckResultDto(
            r.Id, r.EndpointId, r.CheckedAt,
            r.StatusCode, r.ResponseTimeMs, r.IsSuccess, r.ErrorMessage));
    }

    public async Task<EndpointStatsDto?> GetStatsAsync(int id, CancellationToken ct = default)
    {
        var endpoint = await _endpoints.GetByIdAsync(id, ct);
        if (endpoint is null) return null;

        var uptime24h  = await _uptime.CalculateAsync(id, 24, ct);
        var uptime7d   = await _uptime.CalculateAsync(id, 168, ct);
        var uptime30d  = await _uptime.CalculateAsync(id, 720, ct);

        var recentChecks = await _checkResults.GetByEndpointIdAsync(id, 100, ct);
        var avgResponse = recentChecks.Any()
            ? recentChecks.Average(r => r.ResponseTimeMs)
            : 0;

        var allIncidents  = await _incidents.GetByEndpointIdAsync(id, ct);
        var incidentList  = allIncidents.ToList();
        var openIncidents = incidentList.Count(i => i.IsOpen);

        return new EndpointStatsDto(
            EndpointId:          id,
            UptimeLast24Hours:   uptime24h,
            UptimeLast7Days:     uptime7d,
            UptimeLast30Days:    uptime30d,
            AverageResponseTimeMs: Math.Round(avgResponse, 2),
            TotalIncidents:      incidentList.Count,
            OpenIncidents:       openIncidents
        );
    }

    private static EndpointDto MapToDto(MonitoredEndpoint e) => new(
        e.Id, e.Name, e.Url, e.ExpectedStatusCode,
        e.CheckIntervalSeconds, e.IsActive, e.CreatedAt, e.CurrentStatus,
        e.AlertThreshold is null ? null : new AlertThresholdDto(
            e.AlertThreshold.Id,
            e.AlertThreshold.ResponseTimeWarningMs,
            e.AlertThreshold.ResponseTimeCriticalMs,
            e.AlertThreshold.ConsecutiveFailuresDown));
}
