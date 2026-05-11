using EWeaponRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EWeaponRegistry.Infrastructure.Data.Configurations;

public class CitizenProfileConfiguration : IEntityTypeConfiguration<CitizenProfile>
{
    public void Configure(EntityTypeBuilder<CitizenProfile> builder)
    {
        builder.ToTable("citizen_profiles");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        // Encrypted fields - stored as base64 strings
        builder.Property(x => x.FirstNameEncrypted)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.LastNameEncrypted)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.PeselEncrypted)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.AddressEncrypted)
            .HasMaxLength(1024);

        builder.Property(x => x.DocumentNumberEncrypted)
            .HasMaxLength(512);

        builder.Property(x => x.WeaponBookNumberEncrypted)
            .HasMaxLength(512);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
