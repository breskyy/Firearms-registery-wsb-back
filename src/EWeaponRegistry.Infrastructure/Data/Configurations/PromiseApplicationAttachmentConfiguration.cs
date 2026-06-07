using EWeaponRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWeaponRegistry.Infrastructure.Data.Configurations;

public class PromiseApplicationAttachmentConfiguration : IEntityTypeConfiguration<PromiseApplicationAttachment>
{
    public void Configure(EntityTypeBuilder<PromiseApplicationAttachment> builder)
    {
        builder.ToTable("promise_application_attachments");

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

        builder.HasOne(x => x.PromiseApplication)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.PromiseApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.PromiseApplicationId, x.AttachmentType });
    }
}
