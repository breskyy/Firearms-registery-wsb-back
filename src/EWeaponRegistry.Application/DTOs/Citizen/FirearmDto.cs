using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Application.DTOs.Citizen;

public class FirearmDto
{
    public Guid Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public FirearmCategory Category { get; set; }
    public string CategoryName => Category.ToString();
    public string Caliber { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int? ProductionYear { get; set; }
    public FirearmStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime RegisteredAt { get; set; }
}

public class FirearmDetailDto : FirearmDto
{
    public IList<OwnershipHistoryDto> OwnershipHistory { get; set; } = new List<OwnershipHistoryDto>();
}

public class OwnershipHistoryDto
{
    public Guid Id { get; set; }
    public string? PreviousOwnerName { get; set; }
    public string NewOwnerName { get; set; } = string.Empty;
    public TransferType TransferType { get; set; }
    public string TransferTypeName => TransferType.ToString();
    public DateTime TransferDate { get; set; }
    public string? Notes { get; set; }
}
