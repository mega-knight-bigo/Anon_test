using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Infrastructure.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Status).IsRequired().HasDefaultValue("pending");
        builder.Property(e => e.SourceObjects).HasColumnType("jsonb");
        builder.Property(e => e.Mappings).HasColumnType("jsonb");
        builder.Property(e => e.IntegrityRules).HasColumnType("jsonb");
        builder.Property(e => e.Progress).HasDefaultValue(0);
        builder.Property(e => e.RecordsProcessed).HasDefaultValue(0);
        builder.Property(e => e.RecordsTotal).HasDefaultValue(0);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

        builder.HasOne(e => e.SourceConnection)
            .WithMany(c => c.Jobs)
            .HasForeignKey(e => e.SourceConnectionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
