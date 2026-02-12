using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Domain.Interfaces;

public interface IActivityLogRepository
{
    Task<IEnumerable<ActivityLog>> GetRecentAsync(int limit = 50);
    Task<ActivityLog> CreateAsync(ActivityLog log);
}
