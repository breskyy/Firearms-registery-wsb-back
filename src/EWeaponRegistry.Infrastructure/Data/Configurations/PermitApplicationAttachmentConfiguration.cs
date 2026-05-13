using EWeaponRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWeaponRegistry.Infrastructure.Data.Configurations;

public class PermitApplicationAttachmentConfiguration : IEntityTypeConfiguration<PermitApplicationAttachment>
{
    public void Configure(EntityTypeBuilder<PermitApplicationAttachment> builder)
    {
        builder.ToTable("permit_application_attachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AttachmentType)
            .IsRequired();

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.HasOne(x => x.PermitApplication)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.PermitApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.PermitApplicationId, x.AttachmentType });
    }
}
