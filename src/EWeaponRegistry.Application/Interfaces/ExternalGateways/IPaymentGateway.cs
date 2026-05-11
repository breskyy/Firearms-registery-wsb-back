namespace EWeaponRegistry.Application.Interfaces.ExternalGateways;

/// <summary>
/// Gateway for payment operator integration.
/// NOTE: This is a MOCK interface. Real integration would require:
/// - Contract with payment operator
/// - PCI DSS compliance
/// - Secure key management
/// - Webhook handlers for payment notifications
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentInitResult> InitiatePaymentAsync(decimal amount, string description, string referenceId);
    Task<PaymentStatusResult> CheckPaymentStatusAsync(string paymentId);
    Task<PaymentConfirmResult> ConfirmPaymentAsync(string paymentId);
}

public class PaymentInitResult
{
    public bool Success { get; set; }
    public string? PaymentId { get; set; }
    public string? PaymentUrl { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PaymentStatusResult
{
    public bool Found { get; set; }
    public string? Status { get; set; }  // Pending, Completed, Failed, Refunded
    public DateTime? PaidAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PaymentConfirmResult
{
    public bool Success { get; set; }
    public bool IsPaid { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
}
