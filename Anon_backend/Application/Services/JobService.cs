using FullstackTemplate.Application.DTOs;
using FullstackTemplate.Application.Interfaces;
using FullstackTemplate.Domain.Entities;
using FullstackTemplate.Domain.Interfaces;

namespace FullstackTemplate.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepo;
    private readonly IActivityLogRepository _activityLogRepo;

    public JobService(IJobRepository jobRepo, IActivityLogRepository activityLogRepo)
    {
        _jobRepo = jobRepo;
        _activityLogRepo = activityLogRepo;
    }

    public async Task<IEnumerable<JobDto>> GetAllAsync()
    {
        var jobs = await _jobRepo.GetAllAsync();
        return jobs.Select(MapToDto);
    }

    public async Task<JobDto?> GetByIdAsync(Guid id)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        return job is null ? null : MapToDto(job);
    }

    public async Task<JobDto> CreateAsync(CreateJobDto dto)
    {
        var job = new Job
        {
            Name = dto.Name,
            Type = dto.Type,
            SourceConnectionId = dto.SourceConnectionId,
            SourceObjects = dto.SourceObjects,
            Operation = dto.Operation,
            Mappings = dto.Mappings,
            IntegrityRules = dto.IntegrityRules,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _jobRepo.CreateAsync(job);

        await _activityLogRepo.CreateAsync(new ActivityLog
        {
            Action = "created",
            EntityType = "job",
            EntityId = created.Id.ToString(),
            EntityName = created.Name,
            Details = $"New {created.Type} job created"
        });

        return MapToDto(created);
    }

    public async Task<JobDto?> UpdateAsync(Guid id, UpdateJobDto dto)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        if (job is null) return null;

        if (dto.Name is not null) job.Name = dto.Name;
        if (dto.Type is not null) job.Type = dto.Type;
        if (dto.Status is not null) job.Status = dto.Status;
        if (dto.SourceConnectionId is not null) job.SourceConnectionId = dto.SourceConnectionId;
        if (dto.SourceObjects is not null) job.SourceObjects = dto.SourceObjects;
        if (dto.Operation is not null) job.Operation = dto.Operation;
        if (dto.Mappings is not null) job.Mappings = dto.Mappings;
        if (dto.IntegrityRules is not null) job.IntegrityRules = dto.IntegrityRules;
        if (dto.Progress.HasValue) job.Progress = dto.Progress.Value;
        if (dto.RecordsProcessed.HasValue) job.RecordsProcessed = dto.RecordsProcessed.Value;
        if (dto.RecordsTotal.HasValue) job.RecordsTotal = dto.RecordsTotal.Value;
        if (dto.OutputLocation is not null) job.OutputLocation = dto.OutputLocation;
        if (dto.ErrorMessage is not null) job.ErrorMessage = dto.ErrorMessage;
        if (dto.CompletedAt.HasValue) job.CompletedAt = dto.CompletedAt.Value;

        var updated = await _jobRepo.UpdateAsync(job);

        await _activityLogRepo.CreateAsync(new ActivityLog
        {
            Action = "updated",
            EntityType = "job",
            EntityId = updated.Id.ToString(),
            EntityName = updated.Name,
            Details = "Job updated"
        });

        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var job = await _jobRepo.GetByIdAsync(id);
        if (job is null) return false;

        var deleted = await _jobRepo.DeleteAsync(id);
        if (deleted)
        {
            await _activityLogRepo.CreateAsync(new ActivityLog
            {
                Action = "deleted",
                EntityType = "job",
                EntityId = id.ToString(),
                EntityName = job.Name,
                Details = "Job removed"
            });
        }
        return deleted;
    }

    private static JobDto MapToDto(Job j) => new(
        j.Id, j.Name, j.Type, j.Status,
        j.SourceConnectionId, j.SourceObjects,
        j.Operation, j.Mappings, j.IntegrityRules,
        j.Progress, j.RecordsProcessed, j.RecordsTotal,
        j.OutputLocation, j.ErrorMessage,
        j.CreatedAt, j.CompletedAt);
}
