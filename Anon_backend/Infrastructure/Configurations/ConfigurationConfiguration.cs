using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Infrastructure.Configurations;

public class ConfigurationConfiguration : IEntityTypeConfiguration<Configuration>
{
    public void Configure(EntityTypeBuilder<Configuration> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Rules).HasColumnType("jsonb").IsRequired();
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(e => e.Connection)
            .WithMany(c => c.Configurations)
            .HasForeignKey(e => e.ConnectionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.SourceConnection)
            .WithMany(c => c.SourceConfigurations)
            .HasForeignKey(e => e.SourceConnectionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.TargetConnection)
            .WithMany(c => c.TargetConfigurations)
            .HasForeignKey(e => e.TargetConnectionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
