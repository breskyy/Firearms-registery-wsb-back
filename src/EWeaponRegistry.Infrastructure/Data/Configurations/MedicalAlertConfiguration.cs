using EWeaponRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWeaponRegistry.Infrastructure.Data.Configurations;

public class MedicalAlertConfiguration : IEntityTypeConfiguration<MedicalAlert>
{
    public void Configure(EntityTypeBuilder<MedicalAlert> builder)
    {
        builder.ToTable("medical_alerts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AlertType)
            .IsRequired();

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.IsResolved)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(x => x.Citizen)
            .WithMany(x => x.MedicalAlerts)
            .HasForeignKey(x => x.CitizenId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permit)
            .WithMany(x => x.MedicalAlerts)
            .HasForeignKey(x => x.PermitId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
