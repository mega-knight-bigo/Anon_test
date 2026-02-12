namespace FullstackTemplate.Application.DTOs;

public record ActivityLogDto(
    Guid Id,
    string Action,
    string EntityType,
    string? EntityId,
    string? EntityName,
    string? Details,
    DateTime Timestamp
);

public record CreateActivityLogDto(
    string Action,
    string EntityType,
    string? EntityId,
    string? EntityName,
    string? Details
);
