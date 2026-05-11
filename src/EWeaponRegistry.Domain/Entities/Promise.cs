using EWeaponRegistry.Domain.Common;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Domain.Entities;

/// <summary>
/// E-Promise (Promesa) - document authorizing purchase of specific weapon type.
/// </summary>
public class Promise : BaseEntity
{
    public Guid CitizenId { get; set; }
    public Guid PermitId { get; set; }
    public string PromiseNumber { get; set; } = string.Empty;
    public string WeaponType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int UsedQuantity { get; set; }
    public PromiseStatus Status { get; set; }
    public decimal FeeAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? QrToken { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Navigation properties
    public CitizenProfile Citizen { get; set; } = null!;
    public Permit Permit { get; set; } = null!;
}
