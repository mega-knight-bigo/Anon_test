using System.Text.Json;

namespace FullstackTemplate.Application.DTOs;

public record ConnectionMetadataDto(
    Guid Id,
    Guid ConnectionId,
    string ObjectType,
    string ObjectName,
    string? ObjectPath,
    JsonDocument? Columns,
    DateTime FetchedAt
);

public record CreateConnectionMetadataDto(
    string ObjectType,
    string ObjectName,
    string? ObjectPath,
    JsonDocument? Columns
);
