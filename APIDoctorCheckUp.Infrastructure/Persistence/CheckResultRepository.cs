using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIDoctorCheckUp.Infrastructure.Persistence;

public class CheckResultRepository : ICheckResultRepository
{
    private readonly AppDbContext _context;

    public CheckResultRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CheckResult>> GetByEndpointIdAsync(
        int endpointId,
        int limit = 100,
        CancellationToken ct = default)
    {
        return await _context.CheckResults
            .Where(c => c.EndpointId == endpointId)
            .OrderByDescending(c => c.CheckedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<CheckResult?> GetLatestByEndpointIdAsync(
        int endpointId,
        CancellationToken ct = default)
    {
        return await _context.CheckResults
            .Where(c => c.EndpointId == endpointId)
            .OrderByDescending(c => c.CheckedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CheckResult> AddAsync(CheckResult result, CancellationToken ct = default)
    {
        _context.CheckResults.Add(result);
        await _context.SaveChangesAsync(ct);
        return result;
    }

    public async Task<int> GetConsecutiveFailureCountAsync(
        int endpointId,
        CancellationToken ct = default)
    {
        // Retrieve recent results ordered newest-first and count the unbroken
        // failure streak at the head of the list. We fetch a bounded window
        // (capped at ConsecutiveFailuresDown max of ~10) rather than the entire
        // history to keep this query fast regardless of total row count.
        var recentResults = await _context.CheckResults
            .Where(c => c.EndpointId == endpointId)
            .OrderByDescending(c => c.CheckedAt)
            .Take(20)
            .Select(c => c.IsSuccess)
            .ToListAsync(ct);

        var count = 0;
        foreach (var isSuccess in recentResults)
        {
            if (!isSuccess) count++;
            else break;
        }

        return count;
    }
}
