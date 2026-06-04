using EWeaponRegistry.Domain.Entities;
using EWeaponRegistry.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWeaponRegistry.Infrastructure.Data.Configurations;

public class PermitMedicalExamRenewalConfiguration : IEntityTypeConfiguration<PermitMedicalExamRenewal>
{
    public void Configure(EntityTypeBuilder<PermitMedicalExamRenewal> builder)
    {
        builder.ToTable("permit_medical_exam_renewals");

        builder.Property(x => x.ProposedMedicalExpiryDateEncrypted).HasMaxLength(256);
        builder.Property(x => x.ProposedPsychologicalExpiryDateEncrypted).HasMaxLength(256);
        builder.Property(x => x.RejectionReason).HasMaxLength(2000);

        builder.HasOne(x => x.Permit)
            .WithMany()
            .HasForeignKey(x => x.PermitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Citizen)
            .WithMany()
            .HasForeignKey(x => x.CitizenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PermitId)
            .IsUnique()
            .HasDatabaseName("IX_permit_medical_exam_renewals_PermitId_Pending")
            .HasFilter($"\"Status\" IN ({(int)PermitMedicalExamRenewalStatus.Submitted}, {(int)PermitMedicalExamRenewalStatus.UnderReview})");
    }
}
