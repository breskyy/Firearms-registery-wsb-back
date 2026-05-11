using EWeaponRegistry.Domain.Common;

namespace EWeaponRegistry.Domain.Entities;

/// <summary>
/// Licensed firearm shop/dealer.
/// </summary>
public class Shop : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsVerified { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
