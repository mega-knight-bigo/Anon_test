using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FullstackTemplate.Application.DTOs;

public record ConnectionDto(
    Guid Id,
    string Name,
    string Type,
    JsonDocument Config,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateConnectionDto(
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200, ErrorMessage = "Name must be 200 characters or less")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Name contains invalid characters")]
    string Name,
    
    [Required(ErrorMessage = "Type is required")]
    [RegularExpression(@"^(postgres|mysql|snowflake|s3|azure-blob)$", ErrorMessage = "Invalid connection type")]
    string Type,
    
    [Required(ErrorMessage = "Config is required")]
    JsonDocument Config,
    
    [RegularExpression(@"^(active|inactive|testing)$", ErrorMessage = "Invalid status")]
    string? Status
);

public record UpdateConnectionDto(
    [MaxLength(200, ErrorMessage = "Name must be 200 characters or less")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-_]*$", ErrorMessage = "Name contains invalid characters")]
    string? Name,
    
    [RegularExpression(@"^(postgres|mysql|snowflake|s3|azure-blob)$", ErrorMessage = "Invalid connection type")]
    string? Type,
    
    JsonDocument? Config,
    
    [RegularExpression(@"^(active|inactive|testing)$", ErrorMessage = "Invalid status")]
    string? Status
);

public record TestConnectionConfigDto(
    [Required(ErrorMessage = "Type is required")]
    [RegularExpression(@"^(postgres|mysql|snowflake|s3|azure-blob|database)$", ErrorMessage = "Invalid connection type")]
    string Type,
    
    [Required(ErrorMessage = "Config is required")]
    JsonDocument Config
);
