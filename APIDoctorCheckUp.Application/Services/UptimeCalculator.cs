using APIDoctorCheckUp.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace APIDoctorCheckUp.Application.Services;

public class UptimeCalculator : IUptimeCalculator
{
    private readonly ICheckResultRepository _checkResults;
    private readonly ILogger<UptimeCalculator> _logger;

    public UptimeCalculator(
        ICheckResultRepository checkResults,
        ILogger<UptimeCalculator> logger)
    {
        _checkResults = checkResults;
        _logger = logger;
    }

    public async Task<double> CalculateAsync(
        int endpointId,
        int hours,
        CancellationToken ct = default)
    {
        // Fetch a generous window of results rather than filtering by date
        // in the repository, keeping the repository interface simple.
        var results = await _checkResults.GetByEndpointIdAsync(
            endpointId, limit: 10000, ct);

        var cutoff = DateTime.UtcNow.AddHours(-hours);
        var window = results.Where(r => r.CheckedAt >= cutoff).ToList();

        if (window.Count == 0)
        {
            _logger.LogDebug(
                "No check results found for endpoint {EndpointId} in last {Hours}h",
                endpointId, hours);
            return 0;
        }

        var successCount = window.Count(r => r.IsSuccess);
        return Math.Round((double)successCount / window.Count * 100, 2);
    }
}
