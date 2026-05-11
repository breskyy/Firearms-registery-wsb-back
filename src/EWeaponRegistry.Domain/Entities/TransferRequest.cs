using EWeaponRegistry.Domain.Common;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Domain.Entities;

/// <summary>
/// Request for transferring firearm ownership between citizens.
/// </summary>
public class TransferRequest : BaseEntity
{
    public Guid FirearmId { get; set; }
    public Guid SellerCitizenId { get; set; }
    public Guid? BuyerCitizenId { get; set; }
    public string? BuyerPeselEncrypted { get; set; }
    public TransferType TransferType { get; set; }
    public TransferRequestStatus Status { get; set; }
    public DateTime? TransactionDate { get; set; }

    // Navigation properties
    public Firearm Firearm { get; set; } = null!;
    public CitizenProfile Seller { get; set; } = null!;
    public CitizenProfile? Buyer { get; set; }
}
