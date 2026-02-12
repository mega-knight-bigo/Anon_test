using FullstackTemplate.Application.DTOs;
using FullstackTemplate.Application.Interfaces;
using FullstackTemplate.Domain.Interfaces;

namespace FullstackTemplate.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IConnectionRepository _connectionRepo;
    private readonly IJobRepository _jobRepo;

    public DashboardService(IConnectionRepository connectionRepo, IJobRepository jobRepo)
    {
        _connectionRepo = connectionRepo;
        _jobRepo = jobRepo;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var connectionsTask = _connectionRepo.GetAllAsync();
        var jobsTask = _jobRepo.GetAllAsync();

        await Task.WhenAll(connectionsTask, jobsTask);

        var connections = await connectionsTask;
        var jobs = await jobsTask;

        var activeConnections = connections.Count(c => c.Status == "active");
        var completedJobs = jobs.Count(j => j.Status == "completed");
        var runningJobs = jobs.Count(j => j.Status == "running");

        return new DashboardStatsDto(activeConnections, completedJobs, 0, runningJobs);
    }
}
