using EWeaponRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWeaponRegistry.Infrastructure.Data.Configurations;

public class PromiseConfiguration : IEntityTypeConfiguration<Promise>
{
    public void Configure(EntityTypeBuilder<Promise> builder)
    {
        builder.ToTable("promises");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PromiseNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.PromiseNumber)
            .IsUnique();

        builder.Property(x => x.WeaponType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.UsedQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.FeeAmount)
            .HasPrecision(10, 2);

        builder.Property(x => x.PaymentStatus)
            .IsRequired();

        builder.Property(x => x.QrToken)
            .HasMaxLength(256);

        builder.HasIndex(x => x.QrToken);

        builder.HasOne(x => x.Citizen)
            .WithMany(x => x.Promises)
            .HasForeignKey(x => x.CitizenId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permit)
            .WithMany(x => x.Promises)
            .HasForeignKey(x => x.PermitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
