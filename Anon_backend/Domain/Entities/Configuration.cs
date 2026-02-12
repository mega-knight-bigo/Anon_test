using System.Text.Json;

namespace FullstackTemplate.Domain.Entities;

public class Configuration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // internal_consistency, referential_integrity, deidentification
    public string? Description { get; set; }

    // For internal consistency
    public Guid? ConnectionId { get; set; }
    public Connection? Connection { get; set; }
    public string? ObjectName { get; set; }

    // For referential integrity
    public Guid? SourceConnectionId { get; set; }
    public Connection? SourceConnection { get; set; }
    public string? SourceObjectName { get; set; }

    public Guid? TargetConnectionId { get; set; }
    public Connection? TargetConnection { get; set; }
    public string? TargetObjectName { get; set; }

    // Rule details stored as JSON
    public JsonDocument Rules { get; set; } = JsonDocument.Parse("{}");

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
