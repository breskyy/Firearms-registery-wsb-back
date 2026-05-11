using EWeaponRegistry.Domain.Common;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Domain.Entities;

public class OwnershipHistory : BaseEntity
{
    public Guid FirearmId { get; set; }
    public Guid? PreviousOwnerCitizenId { get; set; }
    public Guid NewOwnerCitizenId { get; set; }
    public TransferType TransferType { get; set; }
    public DateTime TransferDate { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Firearm Firearm { get; set; } = null!;
    public CitizenProfile? PreviousOwner { get; set; }
    public CitizenProfile NewOwner { get; set; } = null!;
    public User? CreatedByUser { get; set; }
}
