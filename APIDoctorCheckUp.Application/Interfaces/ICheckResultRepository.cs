using APIDoctorCheckUp.Domain.Entities;

namespace APIDoctorCheckUp.Application.Interfaces;

/// <summary>
/// Defines persistence operations for CheckResult entities.
/// CheckResults are append-only — update and delete are intentionally absent.
/// </summary>
public interface ICheckResultRepository
{
    Task<IEnumerable<CheckResult>> GetByEndpointIdAsync(
        int endpointId,
        int limit = 100,
        CancellationToken ct = default);

    Task<CheckResult?> GetLatestByEndpointIdAsync(
        int endpointId,
        CancellationToken ct = default);

    Task<CheckResult> AddAsync(CheckResult result, CancellationToken ct = default);

    /// <summary>
    /// Returns the count of consecutive failed checks ending at the most recent check.
    /// Used by the alert engine to determine whether to open an incident.
    /// </summary>
    Task<int> GetConsecutiveFailureCountAsync(
        int endpointId,
        CancellationToken ct = default);
}
