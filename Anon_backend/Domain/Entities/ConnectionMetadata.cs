using System.Text.Json;

namespace FullstackTemplate.Domain.Entities;

public class ConnectionMetadata
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConnectionId { get; set; }
    public Connection Connection { get; set; } = null!;
    public string ObjectType { get; set; } = string.Empty; // table, file, folder, bucket
    public string ObjectName { get; set; } = string.Empty;
    public string? ObjectPath { get; set; }
    public JsonDocument? Columns { get; set; } // array of {name, type, nullable}
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}
