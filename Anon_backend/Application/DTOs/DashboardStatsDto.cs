namespace FullstackTemplate.Application.DTOs;

public record DashboardStatsDto(
    int ActiveConnections,
    int JobsProcessed,
    int SecurityAlerts,
    int RunningJobs
);
