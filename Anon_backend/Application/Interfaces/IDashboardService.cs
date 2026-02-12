using FullstackTemplate.Application.DTOs;

namespace FullstackTemplate.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
