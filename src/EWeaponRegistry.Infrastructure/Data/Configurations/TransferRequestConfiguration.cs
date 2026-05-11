using EWeaponRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWeaponRegistry.Infrastructure.Data.Configurations;

public class TransferRequestConfiguration : IEntityTypeConfiguration<TransferRequest>
{
    public void Configure(EntityTypeBuilder<TransferRequest> builder)
    {
        builder.ToTable("transfer_requests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BuyerPeselEncrypted)
            .HasMaxLength(512);

        builder.Property(x => x.TransferType)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasOne(x => x.Firearm)
            .WithMany(x => x.TransferRequests)
            .HasForeignKey(x => x.FirearmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Seller)
            .WithMany(x => x.TransferRequestsAsSeller)
            .HasForeignKey(x => x.SellerCitizenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Buyer)
            .WithMany(x => x.TransferRequestsAsBuyer)
            .HasForeignKey(x => x.BuyerCitizenId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
