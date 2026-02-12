using System.Text.Json;
using FullstackTemplate.Application.DTOs;
using FullstackTemplate.Application.Interfaces;
using FullstackTemplate.Domain.Entities;
using FullstackTemplate.Domain.Interfaces;

namespace FullstackTemplate.Application.Services;

public class ConnectionService : IConnectionService
{
    private readonly IConnectionRepository _connectionRepo;
    private readonly IActivityLogRepository _activityLogRepo;
    private readonly IConnectionMetadataRepository _metadataRepo;
    private readonly ISchemaFetcherFactory _schemaFetcherFactory;
    private readonly IConnectionTesterFactory _connectionTesterFactory;

    public ConnectionService(
        IConnectionRepository connectionRepo,
        IActivityLogRepository activityLogRepo,
        IConnectionMetadataRepository metadataRepo,
        ISchemaFetcherFactory schemaFetcherFactory,
        IConnectionTesterFactory connectionTesterFactory)
    {
        _connectionRepo = connectionRepo;
        _activityLogRepo = activityLogRepo;
        _metadataRepo = metadataRepo;
        _schemaFetcherFactory = schemaFetcherFactory;
        _connectionTesterFactory = connectionTesterFactory;
    }

    public async Task<IEnumerable<ConnectionDto>> GetAllAsync()
    {
        var connections = await _connectionRepo.GetAllAsync();
        return connections.Select(MapToDto);
    }

    public async Task<ConnectionDto?> GetByIdAsync(Guid id)
    {
        var connection = await _connectionRepo.GetByIdAsync(id);
        return connection is null ? null : MapToDto(connection);
    }

    public async Task<ConnectionDto> CreateAsync(CreateConnectionDto dto)
    {
        var connection = new Connection
        {
            Name = dto.Name,
            Type = dto.Type,
            Config = dto.Config,
            Status = dto.Status ?? "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _connectionRepo.CreateAsync(connection);

        await _activityLogRepo.CreateAsync(new ActivityLog
        {
            Action = "created",
            EntityType = "connection",
            EntityId = created.Id.ToString(),
            EntityName = created.Name,
            Details = $"New {created.Type} connection created"
        });

        return MapToDto(created);
    }

    public async Task<ConnectionDto?> UpdateAsync(Guid id, UpdateConnectionDto dto)
    {
        var connection = await _connectionRepo.GetByIdAsync(id);
        if (connection is null) return null;

        if (dto.Name is not null) connection.Name = dto.Name;
        if (dto.Type is not null) connection.Type = dto.Type;
        if (dto.Config is not null) connection.Config = dto.Config;
        if (dto.Status is not null) connection.Status = dto.Status;

        var updated = await _connectionRepo.UpdateAsync(connection);

        await _activityLogRepo.CreateAsync(new ActivityLog
        {
            Action = "updated",
            EntityType = "connection",
            EntityId = updated.Id.ToString(),
            EntityName = updated.Name,
            Details = "Connection updated"
        });

        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var connection = await _connectionRepo.GetByIdAsync(id);
        if (connection is null) return false;

        var deleted = await _connectionRepo.DeleteAsync(id);
        if (deleted)
        {
            await _activityLogRepo.CreateAsync(new ActivityLog
            {
                Action = "deleted",
                EntityType = "connection",
                EntityId = id.ToString(),
                EntityName = connection.Name,
                Details = "Connection removed"
            });
        }
        return deleted;
    }

    public async Task<TestConnectionResult> TestConnectionAsync(Guid id)
    {
        var connection = await _connectionRepo.GetByIdAsync(id);
        if (connection is null) throw new KeyNotFoundException("Connection not found");

        return await TestConnectionConfigAsync(connection.Type, connection.Config);
    }

    public async Task<TestConnectionResult> TestConnectionConfigAsync(string type, JsonDocument config)
    {
        try
        {
            var tester = _connectionTesterFactory.GetTester(type);
            return await tester.TestConnectionAsync(config);
        }
        catch (ArgumentException ex)
        {
            return new TestConnectionResult(false, "Unsupported connection type", ex.Message);
        }
        catch (Exception ex)
        {
            return new TestConnectionResult(false, "Connection test failed", ex.Message);
        }
    }

    public async Task<IEnumerable<ConnectionMetadataDto>> GetMetadataAsync(Guid id)
    {
        var metadata = await _metadataRepo.GetByConnectionIdAsync(id);
        return metadata.Select(MapMetadataToDto);
    }

    public async Task<IEnumerable<ConnectionMetadataDto>> RefreshMetadataAsync(Guid id)
    {
        var connection = await _connectionRepo.GetByIdAsync(id);
        if (connection is null) throw new KeyNotFoundException("Connection not found");

        var fetcher = _schemaFetcherFactory.GetFetcher(connection.Type);
        var fetched = await fetcher.FetchSchemaAsync(connection);

        var saved = await _metadataRepo.SaveAsync(id, fetched);
        return saved.Select(MapMetadataToDto);
    }

    private static ConnectionDto MapToDto(Connection c) => new(
        c.Id, c.Name, c.Type, c.Config, c.Status, c.CreatedAt, c.UpdatedAt);

    private static ConnectionMetadataDto MapMetadataToDto(ConnectionMetadata m) => new(
        m.Id, m.ConnectionId, m.ObjectType, m.ObjectName, m.ObjectPath, m.Columns, m.FetchedAt);
}
