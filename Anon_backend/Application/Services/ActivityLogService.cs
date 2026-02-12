using FullstackTemplate.Application.DTOs;
using FullstackTemplate.Application.Interfaces;
using FullstackTemplate.Domain.Entities;
using FullstackTemplate.Domain.Interfaces;

namespace FullstackTemplate.Application.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogRepository _repo;

    public ActivityLogService(IActivityLogRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<ActivityLogDto>> GetRecentAsync(int limit = 50)
    {
        var logs = await _repo.GetRecentAsync(limit);
        return logs.Select(MapToDto);
    }

    public async Task<ActivityLogDto> CreateAsync(CreateActivityLogDto dto)
    {
        var log = new ActivityLog
        {
            Action = dto.Action,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            EntityName = dto.EntityName,
            Details = dto.Details,
            Timestamp = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(log);
        return MapToDto(created);
    }

    private static ActivityLogDto MapToDto(ActivityLog a) => new(
        a.Id, a.Action, a.EntityType, a.EntityId, a.EntityName, a.Details, a.Timestamp);
}
