using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIDoctorCheckUp.Infrastructure.Persistence;

public class EndpointRepository : IEndpointRepository
{
    private readonly AppDbContext _context;

    public EndpointRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MonitoredEndpoint>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.MonitoredEndpoints
            .Include(e => e.AlertThreshold)
            .OrderBy(e => e.Name)
            .ToListAsync(ct);
    }

    public async Task<MonitoredEndpoint?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.MonitoredEndpoints
            .Include(e => e.AlertThreshold)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<MonitoredEndpoint> AddAsync(MonitoredEndpoint endpoint, CancellationToken ct = default)
    {
        _context.MonitoredEndpoints.Add(endpoint);
        await _context.SaveChangesAsync(ct);
        return endpoint;
    }

    public async Task UpdateAsync(MonitoredEndpoint endpoint, CancellationToken ct = default)
    {
        _context.MonitoredEndpoints.Update(endpoint);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var endpoint = await _context.MonitoredEndpoints.FindAsync([id], ct);
        if (endpoint is not null)
        {
            _context.MonitoredEndpoints.Remove(endpoint);
            await _context.SaveChangesAsync(ct);
        }
    }
}
