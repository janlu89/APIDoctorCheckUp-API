using APIDoctorCheckUp.Application.DTOs;
using APIDoctorCheckUp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace APIDoctorCheckUp.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    /// <summary>
    /// Returns the current status snapshot of all monitored endpoints.
    /// Public — no authentication required. This is what the landing page loads.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var summary = await _dashboard.GetSummaryAsync(ct);
        return Ok(summary);
    }
}
