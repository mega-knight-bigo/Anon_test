namespace FullstackTemplate.Domain.Entities;

public class ActivityLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty; // connection, configuration, job, user
    public string? EntityId { get; set; }
    public string? EntityName { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
