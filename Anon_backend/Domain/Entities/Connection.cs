using System.Text.Json;

namespace FullstackTemplate.Domain.Entities;

public class Connection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // s3, database, azure_blob, snowflake
    public JsonDocument Config { get; set; } = JsonDocument.Parse("{}");
    public string Status { get; set; } = "active"; // active, inactive
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Configuration> Configurations { get; set; } = new List<Configuration>();
    public ICollection<Configuration> SourceConfigurations { get; set; } = new List<Configuration>();
    public ICollection<Configuration> TargetConfigurations { get; set; } = new List<Configuration>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public ICollection<ConnectionMetadata> Metadata { get; set; } = new List<ConnectionMetadata>();
}
