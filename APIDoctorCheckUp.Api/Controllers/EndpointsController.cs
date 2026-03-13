using APIDoctorCheckUp.Application.DTOs;
using APIDoctorCheckUp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIDoctorCheckUp.Api.Controllers;

[ApiController]
[Route("api/endpoints")]
public class EndpointsController : ControllerBase
{
    private readonly IEndpointService _endpointService;
    private readonly IMonitoringOrchestrator _orchestrator;

    public EndpointsController(
        IEndpointService endpointService,
        IMonitoringOrchestrator orchestrator)
    {
        _endpointService = endpointService;
        _orchestrator    = orchestrator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EndpointDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var endpoints = await _endpointService.GetAllAsync(ct);
        return Ok(endpoints);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EndpointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var endpoint = await _endpointService.GetByIdAsync(id, ct);
        return endpoint is null ? NotFound() : Ok(endpoint);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(EndpointDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateEndpointDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _endpointService.CreateAsync(dto, ct);

        // Signal the orchestrator to start monitoring the new endpoint immediately
        await _orchestrator.StartEndpointAsync(created.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(EndpointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateEndpointDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _endpointService.UpdateAsync(id, dto, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        // Stop the worker before deleting so the worker does not attempt
        // a check on an endpoint that no longer exists in the database
        await _orchestrator.StopEndpointAsync(id);

        var deleted = await _endpointService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/checks")]
    [ProducesResponseType(typeof(IEnumerable<CheckResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChecks(
        int id, [FromQuery] int limit = 100, CancellationToken ct = default)
    {
        var endpoint = await _endpointService.GetByIdAsync(id, ct);
        if (endpoint is null) return NotFound();

        var checks = await _endpointService.GetChecksAsync(id, Math.Clamp(limit, 1, 500), ct);
        return Ok(checks);
    }

    [HttpGet("{id:int}/stats")]
    [ProducesResponseType(typeof(EndpointStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStats(int id, CancellationToken ct)
    {
        var stats = await _endpointService.GetStatsAsync(id, ct);
        return stats is null ? NotFound() : Ok(stats);
    }
}
