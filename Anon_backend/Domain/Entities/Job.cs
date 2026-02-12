using System.Text.Json;

namespace FullstackTemplate.Domain.Entities;

public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // deidentify, subset, synthesize
    public string Status { get; set; } = "pending"; // pending, running, completed, failed

    // Job configuration
    public Guid? SourceConnectionId { get; set; }
    public Connection? SourceConnection { get; set; }
    public JsonDocument? SourceObjects { get; set; } // selected tables/files

    // De-identification settings
    public string? Operation { get; set; } // anonymize, pseudonymize
    public JsonDocument? Mappings { get; set; } // field-level anonymization strategies
    public JsonDocument? IntegrityRules { get; set; } // applied configuration IDs

    // Progress tracking
    public int Progress { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsTotal { get; set; }

    // Results
    public string? OutputLocation { get; set; }
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
