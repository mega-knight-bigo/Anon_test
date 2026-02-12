using Microsoft.EntityFrameworkCore;
using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<Configuration> Configurations => Set<Configuration>();
    public DbSet<Job> Jobs => Set<Job>();

    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<ConnectionMetadata> ConnectionMetadata => Set<ConnectionMetadata>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
