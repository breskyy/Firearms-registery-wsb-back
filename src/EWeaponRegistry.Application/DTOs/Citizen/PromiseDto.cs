using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Application.DTOs.Citizen;

public class PromiseDto
{
    public Guid Id { get; set; }
    public string PromiseNumber { get; set; } = string.Empty;
    public string WeaponType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int UsedQuantity { get; set; }
    public int RemainingQuantity => Quantity - UsedQuantity;
    public PromiseStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public decimal FeeAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusName => PaymentStatus.ToString();
    public string? QrToken { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsValid => (Status == PromiseStatus.Active || Status == PromiseStatus.Approved)
                           && ExpiryDate >= DateTime.UtcNow.Date
                           && RemainingQuantity > 0;
}
