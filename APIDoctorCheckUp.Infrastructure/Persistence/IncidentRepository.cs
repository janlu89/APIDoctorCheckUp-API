using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIDoctorCheckUp.Infrastructure.Persistence;

public class IncidentRepository : IIncidentRepository
{
    private readonly AppDbContext _context;

    public IncidentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Incident>> GetByEndpointIdAsync(
        int endpointId,
        CancellationToken ct = default)
    {
        return await _context.Incidents
            .Where(i => i.EndpointId == endpointId)
            .OrderByDescending(i => i.StartedAt)
            .ToListAsync(ct);
    }

    public async Task<Incident?> GetOpenIncidentAsync(
        int endpointId,
        CancellationToken ct = default)
    {
        return await _context.Incidents
            .Where(i => i.EndpointId == endpointId && i.ResolvedAt == null)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Incident> AddAsync(Incident incident, CancellationToken ct = default)
    {
        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync(ct);
        return incident;
    }

    public async Task UpdateAsync(Incident incident, CancellationToken ct = default)
    {
        _context.Incidents.Update(incident);
        await _context.SaveChangesAsync(ct);
    }
}
