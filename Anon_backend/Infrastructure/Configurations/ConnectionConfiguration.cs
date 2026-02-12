using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Infrastructure.Configurations;

public class ConnectionConfiguration : IEntityTypeConfiguration<Connection>
{
    public void Configure(EntityTypeBuilder<Connection> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Config).HasColumnType("jsonb").IsRequired();
        builder.Property(e => e.Status).IsRequired().HasDefaultValue("active");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
    }
}
