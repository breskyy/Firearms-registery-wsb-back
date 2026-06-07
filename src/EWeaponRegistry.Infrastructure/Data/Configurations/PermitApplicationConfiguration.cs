using EWeaponRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWeaponRegistry.Infrastructure.Data.Configurations;

public class PermitApplicationConfiguration : IEntityTypeConfiguration<PermitApplication>
{
    public void Configure(EntityTypeBuilder<PermitApplication> builder)
    {
        builder.ToTable("permit_applications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.FeeAmount)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.PaymentStatus)
            .IsRequired();

        builder.Property(x => x.PaymentReferenceId)
            .HasMaxLength(64);

        builder.Property(x => x.PaymentRejectionComment)
            .HasMaxLength(1000);

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(x => x.CorrectionNotes)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Citizen)
            .WithMany(x => x.PermitApplications)
            .HasForeignKey(x => x.CitizenId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReviewedByOfficer)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByOfficerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.GeneratedPermit)
            .WithMany()
            .HasForeignKey(x => x.GeneratedPermitId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
