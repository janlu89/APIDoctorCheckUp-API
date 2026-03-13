using APIDoctorCheckUp.Application.DTOs;

namespace APIDoctorCheckUp.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
}
