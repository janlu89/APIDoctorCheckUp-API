using APIDoctorCheckUp.Domain.Entities;

namespace APIDoctorCheckUp.Application.Interfaces;

/// <summary>
/// Defines persistence operations for MonitoredEndpoint entities.
/// The Infrastructure layer provides the EF Core implementation.
/// </summary>
public interface IEndpointRepository
{
    Task<IEnumerable<MonitoredEndpoint>> GetAllAsync(CancellationToken ct = default);
    Task<MonitoredEndpoint?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MonitoredEndpoint> AddAsync(MonitoredEndpoint endpoint, CancellationToken ct = default);
    Task UpdateAsync(MonitoredEndpoint endpoint, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
