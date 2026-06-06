using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Application.DTOs.Citizen;

public class ApplicationPaymentDto
{
    public Guid ApplicationId { get; set; }
    public decimal FeeAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusName => PaymentStatus.ToString();
    public string? PaymentReferenceId { get; set; }
    public string? PaymentUrl { get; set; }
}

public class ConfirmApplicationPaymentRequest
{
    public string PaymentId { get; set; } = string.Empty;
}
