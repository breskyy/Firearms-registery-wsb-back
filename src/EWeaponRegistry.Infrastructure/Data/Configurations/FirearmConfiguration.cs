using EWeaponRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWeaponRegistry.Infrastructure.Data.Configurations;

public class FirearmConfiguration : IEntityTypeConfiguration<Firearm>
{
    public void Configure(EntityTypeBuilder<Firearm> builder)
    {
        builder.ToTable("firearms");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Category)
            .IsRequired();

        builder.Property(x => x.Caliber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SerialNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.SerialNumber)
            .IsUnique();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.RegisteredAt)
            .IsRequired();

        builder.HasOne(x => x.Owner)
            .WithMany(x => x.Firearms)
            .HasForeignKey(x => x.OwnerCitizenId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
