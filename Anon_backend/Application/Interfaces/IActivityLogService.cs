using FullstackTemplate.Application.DTOs;

namespace FullstackTemplate.Application.Interfaces;

public interface IActivityLogService
{
    Task<IEnumerable<ActivityLogDto>> GetRecentAsync(int limit = 50);
    Task<ActivityLogDto> CreateAsync(CreateActivityLogDto dto);
}
