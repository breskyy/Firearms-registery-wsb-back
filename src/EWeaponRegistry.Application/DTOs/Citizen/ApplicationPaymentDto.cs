using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Application.DTOs.Citizen;

public class ApplicationPaymentDto
{
    public Guid ApplicationId { get; set; }
    public decimal FeeAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusName => PaymentStatus.ToString();
    public PaymentMethod? PaymentMethod { get; set; }
    public string? PaymentMethodName => PaymentMethod?.ToString();
    public string? PaymentReferenceId { get; set; }
    public string? PaymentUrl { get; set; }
    public BankTransferDetailsDto? BankTransferDetails { get; set; }
    public string? PaymentRejectionComment { get; set; }
}

public class BankTransferDetailsDto
{
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string TransferTitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class ConfirmApplicationPaymentRequest
{
    public string PaymentId { get; set; } = string.Empty;
}
