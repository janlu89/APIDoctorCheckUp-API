using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace APIDoctorCheckUp.Infrastructure.Http;

public class EndpointChecker : IEndpointChecker
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EndpointChecker> _logger;

    // Named client configured in MonitoringExtensions with appropriate timeouts
    private const string HttpClientName = "MonitoringClient";

    public EndpointChecker(IHttpClientFactory httpClientFactory, ILogger<EndpointChecker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<CheckResult> CheckAsync(MonitoredEndpoint endpoint, CancellationToken ct = default)
    {
        var client    = _httpClientFactory.CreateClient(HttpClientName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await client.GetAsync(endpoint.Url, ct);
            stopwatch.Stop();

            var isSuccess = (int)response.StatusCode == endpoint.ExpectedStatusCode;

            _logger.LogDebug(
                "Checked {EndpointName} — {StatusCode} in {ResponseTimeMs}ms — {Result}",
                endpoint.Name, (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds, isSuccess ? "OK" : "FAIL");

            return new CheckResult
            {
                EndpointId      = endpoint.Id,
                CheckedAt       = DateTime.UtcNow,
                StatusCode      = (int)response.StatusCode,
                ResponseTimeMs  = stopwatch.ElapsedMilliseconds,
                IsSuccess       = isSuccess,
                ErrorMessage    = isSuccess ? null
                    : $"Expected status {endpoint.ExpectedStatusCode}, got {(int)response.StatusCode}"
            };
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            // The HttpClient timeout fired, not the application shutdown token
            stopwatch.Stop();
            _logger.LogWarning("Timeout checking {EndpointName} after {Ms}ms",
                endpoint.Name, stopwatch.ElapsedMilliseconds);

            return new CheckResult
            {
                EndpointId     = endpoint.Id,
                CheckedAt      = DateTime.UtcNow,
                StatusCode     = null,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                IsSuccess      = false,
                ErrorMessage   = "Request timed out."
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning("HTTP error checking {EndpointName}: {Message}",
                endpoint.Name, ex.Message);

            return new CheckResult
            {
                EndpointId     = endpoint.Id,
                CheckedAt      = DateTime.UtcNow,
                StatusCode     = null,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                IsSuccess      = false,
                ErrorMessage   = ex.Message
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Unexpected error checking {EndpointName}", endpoint.Name);

            return new CheckResult
            {
                EndpointId     = endpoint.Id,
                CheckedAt      = DateTime.UtcNow,
                StatusCode     = null,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                IsSuccess      = false,
                ErrorMessage   = $"Unexpected error: {ex.Message}"
            };
        }
    }
}
