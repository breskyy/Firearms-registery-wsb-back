using EWeaponRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWeaponRegistry.Infrastructure.Data.Configurations;

public class PermitConfiguration : IEntityTypeConfiguration<Permit>
{
    public void Configure(EntityTypeBuilder<Permit> builder)
    {
        builder.ToTable("permits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PermitNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.PermitNumber)
            .IsUnique();

        builder.Property(x => x.PermitType)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.IssueDate)
            .IsRequired();

        builder.Property(x => x.ExpiryDate)
            .IsRequired();

        builder.Property(x => x.MaxFirearms)
            .IsRequired();

        builder.Property(x => x.UsedSlots)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.MedicalExamExpiryDateEncrypted)
            .HasMaxLength(256);

        builder.Property(x => x.PsychologicalExamExpiryDateEncrypted)
            .HasMaxLength(256);

        builder.HasOne(x => x.Citizen)
            .WithMany(x => x.Permits)
            .HasForeignKey(x => x.CitizenId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
