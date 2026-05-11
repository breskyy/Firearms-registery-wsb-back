using EWeaponRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWeaponRegistry.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserRole)
            .HasMaxLength(50);

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.EntityType)
            .HasMaxLength(100);

        builder.Property(x => x.EntityId)
            .HasMaxLength(100);

        builder.Property(x => x.TimestampUtc)
            .IsRequired();

        builder.Property(x => x.IpAddress)
            .HasMaxLength(50);

        builder.Property(x => x.OldValuesJson)
            .HasColumnType("jsonb");

        builder.Property(x => x.NewValuesJson)
            .HasColumnType("jsonb");

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        // Indexes for common queries
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.TimestampUtc);
        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
