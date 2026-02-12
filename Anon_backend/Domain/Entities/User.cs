namespace FullstackTemplate.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Auth fields
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfileImageUrl { get; set; }

    // App-specific fields
    public string? Name { get; set; }
    public string Role { get; set; } = "viewer"; // admin, developer, scheduler, viewer
    public string Status { get; set; } = "active"; // active, inactive
    public DateOnly? InactiveDate { get; set; }
    public string? Department { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
