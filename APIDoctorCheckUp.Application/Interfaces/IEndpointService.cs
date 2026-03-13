using APIDoctorCheckUp.Application.DTOs;

namespace APIDoctorCheckUp.Application.Interfaces;

public interface IEndpointService
{
    Task<IEnumerable<EndpointDto>> GetAllAsync(CancellationToken ct = default);
    Task<EndpointDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<EndpointDto> CreateAsync(CreateEndpointDto dto, CancellationToken ct = default);
    Task<EndpointDto?> UpdateAsync(int id, UpdateEndpointDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<CheckResultDto>> GetChecksAsync(int id, int limit, CancellationToken ct = default);
    Task<EndpointStatsDto?> GetStatsAsync(int id, CancellationToken ct = default);
}
