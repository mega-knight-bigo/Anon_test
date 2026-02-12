using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FullstackTemplate.Application.DTOs;

public record ConfigurationDto(
    Guid Id,
    string Name,
    string Type,
    string? Description,
    Guid? ConnectionId,
    string? ObjectName,
    Guid? SourceConnectionId,
    string? SourceObjectName,
    Guid? TargetConnectionId,
    string? TargetObjectName,
    JsonDocument Rules,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateConfigurationDto(
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200, ErrorMessage = "Name must be 200 characters or less")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Name contains invalid characters")]
    string Name,
    
    [Required(ErrorMessage = "Type is required")]
    [RegularExpression(@"^(masking|anonymization|encryption|tokenization)$", ErrorMessage = "Invalid configuration type")]
    string Type,
    
    [MaxLength(1000, ErrorMessage = "Description must be 1000 characters or less")]
    string? Description,
    
    Guid? ConnectionId,
    
    [MaxLength(500, ErrorMessage = "Object name must be 500 characters or less")]
    [RegularExpression(@"^[a-zA-Z0-9\._\-]+$", ErrorMessage = "Object name contains invalid characters")]
    string? ObjectName,
    
    Guid? SourceConnectionId,
    
    [MaxLength(500, ErrorMessage = "Source object name must be 500 characters or less")]
    [RegularExpression(@"^[a-zA-Z0-9\._\-]+$", ErrorMessage = "Source object name contains invalid characters")]
    string? SourceObjectName,
    
    Guid? TargetConnectionId,
    
    [MaxLength(500, ErrorMessage = "Target object name must be 500 characters or less")]
    [RegularExpression(@"^[a-zA-Z0-9\._\-]+$", ErrorMessage = "Target object name contains invalid characters")]
    string? TargetObjectName,
    
    [Required(ErrorMessage = "Rules are required")]
    JsonDocument Rules
);

public record UpdateConfigurationDto(
    [MaxLength(200, ErrorMessage = "Name must be 200 characters or less")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-_]*$", ErrorMessage = "Name contains invalid characters")]
    string? Name,
    
    [RegularExpression(@"^(masking|anonymization|encryption|tokenization)$", ErrorMessage = "Invalid configuration type")]
    string? Type,
    
    [MaxLength(1000, ErrorMessage = "Description must be 1000 characters or less")]
    string? Description,
    
    Guid? ConnectionId,
    
    [MaxLength(500, ErrorMessage = "Object name must be 500 characters or less")]
    [RegularExpression(@"^[a-zA-Z0-9\._\-]*$", ErrorMessage = "Object name contains invalid characters")]
    string? ObjectName,
    
    Guid? SourceConnectionId,
    
    [MaxLength(500, ErrorMessage = "Source object name must be 500 characters or less")]
    [RegularExpression(@"^[a-zA-Z0-9\._\-]*$", ErrorMessage = "Source object name contains invalid characters")]
    string? SourceObjectName,
    
    Guid? TargetConnectionId,
    
    [MaxLength(500, ErrorMessage = "Target object name must be 500 characters or less")]
    [RegularExpression(@"^[a-zA-Z0-9\._\-]*$", ErrorMessage = "Target object name contains invalid characters")]
    string? TargetObjectName,
    
    JsonDocument? Rules
);
