using EWeaponRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWeaponRegistry.Infrastructure.Data.Configurations;

public class OwnershipHistoryConfiguration : IEntityTypeConfiguration<OwnershipHistory>
{
    public void Configure(EntityTypeBuilder<OwnershipHistory> builder)
    {
        builder.ToTable("ownership_histories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransferType)
            .IsRequired();

        builder.Property(x => x.TransferDate)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Firearm)
            .WithMany(x => x.OwnershipHistories)
            .HasForeignKey(x => x.FirearmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PreviousOwner)
            .WithMany()
            .HasForeignKey(x => x.PreviousOwnerCitizenId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.NewOwner)
            .WithMany()
            .HasForeignKey(x => x.NewOwnerCitizenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
