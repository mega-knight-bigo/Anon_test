using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Infrastructure.Configurations;

public class ConnectionMetadataConfiguration : IEntityTypeConfiguration<ConnectionMetadata>
{
    public void Configure(EntityTypeBuilder<ConnectionMetadata> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.ConnectionId).IsRequired();
        builder.Property(e => e.ObjectType).IsRequired();
        builder.Property(e => e.ObjectName).IsRequired();
        builder.Property(e => e.Columns).HasColumnType("jsonb");
        builder.Property(e => e.FetchedAt).HasDefaultValueSql("now()");

        builder.HasOne(e => e.Connection)
            .WithMany(c => c.Metadata)
            .HasForeignKey(e => e.ConnectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
