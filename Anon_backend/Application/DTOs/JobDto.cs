using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FullstackTemplate.Application.DTOs;

public record JobDto(
    Guid Id,
    string Name,
    string Type,
    string Status,
    Guid? SourceConnectionId,
    JsonDocument? SourceObjects,
    string? Operation,
    JsonDocument? Mappings,
    JsonDocument? IntegrityRules,
    int Progress,
    int RecordsProcessed,
    int RecordsTotal,
    string? OutputLocation,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

public record CreateJobDto(
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200, ErrorMessage = "Name must be 200 characters or less")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Name contains invalid characters")]
    string Name,
    
    [Required(ErrorMessage = "Type is required")]
    [RegularExpression(@"^(anonymization|masking|tokenization|encryption|synthetic)$", ErrorMessage = "Invalid job type")]
    string Type,
    
    Guid? SourceConnectionId,
    JsonDocument? SourceObjects,
    
    [RegularExpression(@"^(mask|hash|encrypt|tokenize|redact|generate)$", ErrorMessage = "Invalid operation")]
    string? Operation,
    
    JsonDocument? Mappings,
    JsonDocument? IntegrityRules
);

public record UpdateJobDto(
    [MaxLength(200, ErrorMessage = "Name must be 200 characters or less")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-_]*$", ErrorMessage = "Name contains invalid characters")]
    string? Name,
    
    [RegularExpression(@"^(anonymization|masking|tokenization|encryption|synthetic)$", ErrorMessage = "Invalid job type")]
    string? Type,
    
    [RegularExpression(@"^(pending|running|completed|failed|cancelled)$", ErrorMessage = "Invalid status")]
    string? Status,
    
    Guid? SourceConnectionId,
    JsonDocument? SourceObjects,
    
    [RegularExpression(@"^(mask|hash|encrypt|tokenize|redact|generate)$", ErrorMessage = "Invalid operation")]
    string? Operation,
    
    JsonDocument? Mappings,
    JsonDocument? IntegrityRules,
    
    [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
    int? Progress,
    
    [Range(0, int.MaxValue, ErrorMessage = "Records processed must be non-negative")]
    int? RecordsProcessed,
    
    [Range(0, int.MaxValue, ErrorMessage = "Records total must be non-negative")]
    int? RecordsTotal,
    
    [MaxLength(2048, ErrorMessage = "Output location must be 2048 characters or less")]
    string? OutputLocation,
    
    [MaxLength(4000, ErrorMessage = "Error message must be 4000 characters or less")]
    string? ErrorMessage,
    
    DateTime? CompletedAt
);
