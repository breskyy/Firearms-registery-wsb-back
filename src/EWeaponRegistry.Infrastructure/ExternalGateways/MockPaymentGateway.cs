using EWeaponRegistry.Application.Interfaces.ExternalGateways;
using Microsoft.Extensions.Logging;

namespace EWeaponRegistry.Infrastructure.ExternalGateways;

/// <summary>
/// Mock implementation of Payment Gateway.
/// NOTE: This is a DEMONSTRATION ONLY.
/// Real integration with a payment operator would require:
/// - Contract with payment operator (e.g., PayU, Przelewy24, Stripe)
/// - PCI DSS compliance
/// - Secure key management
/// - Webhook handlers for payment notifications
/// </summary>
public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;

    // In-memory store for demo purposes
    private static readonly Dictionary<string, (decimal Amount, string Status, DateTime? PaidAt)> _payments = new();

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    public Task<PaymentInitResult> InitiatePaymentAsync(decimal amount, string description, string referenceId)
    {
        _logger.LogInformation("[MOCK] Payment initiation requested: {Amount} PLN for {Description}", amount, description);

        var paymentId = $"PAY-{Guid.NewGuid():N}"[..20].ToUpper();

        _payments[paymentId] = (amount, "Pending", null);

        return Task.FromResult(new PaymentInitResult
        {
            Success = true,
            PaymentId = paymentId,
            PaymentUrl = $"https://mock-payment.example.com/pay/{paymentId}"
        });
    }

    public Task<PaymentStatusResult> CheckPaymentStatusAsync(string paymentId)
    {
        _logger.LogInformation("[MOCK] Payment status check requested for: {PaymentId}", paymentId);

        if (_payments.TryGetValue(paymentId, out var payment))
        {
            return Task.FromResult(new PaymentStatusResult
            {
                Found = true,
                Status = payment.Status,
                PaidAt = payment.PaidAt
            });
        }

        return Task.FromResult(new PaymentStatusResult
        {
            Found = false,
            ErrorMessage = "Payment not found"
        });
    }

    public Task<PaymentConfirmResult> ConfirmPaymentAsync(string paymentId)
    {
        _logger.LogInformation("[MOCK] Payment confirmation requested for: {PaymentId}", paymentId);

        if (_payments.TryGetValue(paymentId, out var payment))
        {
            // Simulate successful payment
            _payments[paymentId] = (payment.Amount, "Completed", DateTime.UtcNow);

            return Task.FromResult(new PaymentConfirmResult
            {
                Success = true,
                IsPaid = true,
                TransactionId = $"TXN-{Guid.NewGuid():N}"[..16].ToUpper()
            });
        }

        return Task.FromResult(new PaymentConfirmResult
        {
            Success = false,
            IsPaid = false,
            ErrorMessage = "Payment not found"
        });
    }
}
