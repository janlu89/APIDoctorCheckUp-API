using APIDoctorCheckUp.Domain.Entities;

namespace APIDoctorCheckUp.Application.Interfaces;

/// <summary>
/// Defines persistence operations for Incident entities.
/// </summary>
public interface IIncidentRepository
{
    Task<IEnumerable<Incident>> GetByEndpointIdAsync(
        int endpointId,
        CancellationToken ct = default);

    Task<Incident?> GetOpenIncidentAsync(
        int endpointId,
        CancellationToken ct = default);

    Task<Incident> AddAsync(Incident incident, CancellationToken ct = default);
    Task UpdateAsync(Incident incident, CancellationToken ct = default);
}
