using FullstackTemplate.Application.DTOs;
using FullstackTemplate.Application.Interfaces;
using FullstackTemplate.Domain.Entities;
using FullstackTemplate.Domain.Interfaces;

namespace FullstackTemplate.Application.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configRepo;
    private readonly IActivityLogRepository _activityLogRepo;

    public ConfigurationService(IConfigurationRepository configRepo, IActivityLogRepository activityLogRepo)
    {
        _configRepo = configRepo;
        _activityLogRepo = activityLogRepo;
    }

    public async Task<IEnumerable<ConfigurationDto>> GetAllAsync(string? type = null)
    {
        var configs = await _configRepo.GetAllAsync(type);
        return configs.Select(MapToDto);
    }

    public async Task<ConfigurationDto?> GetByIdAsync(Guid id)
    {
        var config = await _configRepo.GetByIdAsync(id);
        return config is null ? null : MapToDto(config);
    }

    public async Task<ConfigurationDto> CreateAsync(CreateConfigurationDto dto)
    {
        var config = new Configuration
        {
            Name = dto.Name,
            Type = dto.Type,
            Description = dto.Description,
            ConnectionId = dto.ConnectionId,
            ObjectName = dto.ObjectName,
            SourceConnectionId = dto.SourceConnectionId,
            SourceObjectName = dto.SourceObjectName,
            TargetConnectionId = dto.TargetConnectionId,
            TargetObjectName = dto.TargetObjectName,
            Rules = dto.Rules,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _configRepo.CreateAsync(config);

        await _activityLogRepo.CreateAsync(new ActivityLog
        {
            Action = "created",
            EntityType = "configuration",
            EntityId = created.Id.ToString(),
            EntityName = created.Name,
            Details = $"New {created.Type} configuration created"
        });

        return MapToDto(created);
    }

    public async Task<ConfigurationDto?> UpdateAsync(Guid id, UpdateConfigurationDto dto)
    {
        var config = await _configRepo.GetByIdAsync(id);
        if (config is null) return null;

        if (dto.Name is not null) config.Name = dto.Name;
        if (dto.Type is not null) config.Type = dto.Type;
        if (dto.Description is not null) config.Description = dto.Description;
        if (dto.ConnectionId is not null) config.ConnectionId = dto.ConnectionId;
        if (dto.ObjectName is not null) config.ObjectName = dto.ObjectName;
        if (dto.SourceConnectionId is not null) config.SourceConnectionId = dto.SourceConnectionId;
        if (dto.SourceObjectName is not null) config.SourceObjectName = dto.SourceObjectName;
        if (dto.TargetConnectionId is not null) config.TargetConnectionId = dto.TargetConnectionId;
        if (dto.TargetObjectName is not null) config.TargetObjectName = dto.TargetObjectName;
        if (dto.Rules is not null) config.Rules = dto.Rules;

        var updated = await _configRepo.UpdateAsync(config);

        await _activityLogRepo.CreateAsync(new ActivityLog
        {
            Action = "updated",
            EntityType = "configuration",
            EntityId = updated.Id.ToString(),
            EntityName = updated.Name,
            Details = "Configuration updated"
        });

        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var config = await _configRepo.GetByIdAsync(id);
        if (config is null) return false;

        var deleted = await _configRepo.DeleteAsync(id);
        if (deleted)
        {
            await _activityLogRepo.CreateAsync(new ActivityLog
            {
                Action = "deleted",
                EntityType = "configuration",
                EntityId = id.ToString(),
                EntityName = config.Name,
                Details = "Configuration removed"
            });
        }
        return deleted;
    }

    private static ConfigurationDto MapToDto(Configuration c) => new(
        c.Id, c.Name, c.Type, c.Description,
        c.ConnectionId, c.ObjectName,
        c.SourceConnectionId, c.SourceObjectName,
        c.TargetConnectionId, c.TargetObjectName,
        c.Rules, c.CreatedAt, c.UpdatedAt);
}
