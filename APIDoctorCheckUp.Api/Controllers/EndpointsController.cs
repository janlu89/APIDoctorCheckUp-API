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

    public EndpointsController(IEndpointService endpointService)
    {
        _endpointService = endpointService;
    }

    /// <summary>Returns all monitored endpoints. Public.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EndpointDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var endpoints = await _endpointService.GetAllAsync(ct);
        return Ok(endpoints);
    }

    /// <summary>Returns a single endpoint by ID. Public.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EndpointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var endpoint = await _endpointService.GetByIdAsync(id, ct);
        return endpoint is null ? NotFound() : Ok(endpoint);
    }

    /// <summary>Creates a new monitored endpoint. Requires JWT.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(EndpointDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateEndpointDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _endpointService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing endpoint. Requires JWT.</summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(EndpointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateEndpointDto dto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _endpointService.UpdateAsync(id, dto, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// Deletes an endpoint and all its history. Requires JWT.
    /// The background worker for this endpoint will be stopped on Day 4.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _endpointService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Returns historical check results for an endpoint. Public.</summary>
    [HttpGet("{id:int}/checks")]
    [ProducesResponseType(typeof(IEnumerable<CheckResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChecks(
        int id,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var endpoint = await _endpointService.GetByIdAsync(id, ct);
        if (endpoint is null) return NotFound();

        var checks = await _endpointService.GetChecksAsync(id, Math.Clamp(limit, 1, 500), ct);
        return Ok(checks);
    }

    /// <summary>Returns uptime statistics for an endpoint. Public.</summary>
    [HttpGet("{id:int}/stats")]
    [ProducesResponseType(typeof(EndpointStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStats(int id, CancellationToken ct)
    {
        var stats = await _endpointService.GetStatsAsync(id, ct);
        return stats is null ? NotFound() : Ok(stats);
    }
}
