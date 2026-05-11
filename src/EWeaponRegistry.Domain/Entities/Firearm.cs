using EWeaponRegistry.Domain.Common;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Domain.Entities;

public class Firearm : BaseEntity
{
    public Guid OwnerCitizenId { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public FirearmCategory Category { get; set; }
    public string Caliber { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int? ProductionYear { get; set; }
    public FirearmStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }

    // Navigation properties
    public CitizenProfile Owner { get; set; } = null!;
    public ICollection<OwnershipHistory> OwnershipHistories { get; set; } = new List<OwnershipHistory>();
    public ICollection<TransferRequest> TransferRequests { get; set; } = new List<TransferRequest>();
}
