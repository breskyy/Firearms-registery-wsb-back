using EWeaponRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWeaponRegistry.Infrastructure.Data.Configurations;

public class PermitMedicalExamRenewalAttachmentConfiguration : IEntityTypeConfiguration<PermitMedicalExamRenewalAttachment>
{
    public void Configure(EntityTypeBuilder<PermitMedicalExamRenewalAttachment> builder)
    {
        builder.ToTable("permit_medical_exam_renewal_attachments");

        builder.Property(x => x.FileName).HasMaxLength(255);
        builder.Property(x => x.ContentType).HasMaxLength(100);

        builder.HasOne(x => x.Renewal)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.PermitMedicalExamRenewalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.PermitMedicalExamRenewalId, x.AttachmentType });
    }
}
