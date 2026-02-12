using Microsoft.EntityFrameworkCore;
using FullstackTemplate.Domain.Entities;
using FullstackTemplate.Domain.Interfaces;
using FullstackTemplate.Infrastructure.Data;

namespace FullstackTemplate.Infrastructure.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly ApplicationDbContext _context;

    public ActivityLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ActivityLog>> GetRecentAsync(int limit = 50)
    {
        return await _context.ActivityLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<ActivityLog> CreateAsync(ActivityLog log)
    {
        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }
}
