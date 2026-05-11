using EWeaponRegistry.Application.Interfaces.ExternalGateways;
using Microsoft.Extensions.Logging;

namespace EWeaponRegistry.Infrastructure.ExternalGateways;

/// <summary>
/// Mock implementation of mObywatel Gateway.
/// NOTE: This is a DEMONSTRATION ONLY.
/// Real integration with mObywatel would require:
/// - Access to mObywatel API
/// - Proper authentication and authorization
/// - Digital document handling
/// - HTTPS/TLS communication
/// </summary>
public class MockMObywatelGateway : IMObywatelGateway
{
    private readonly ILogger<MockMObywatelGateway> _logger;

    // In-memory store for demo purposes
    private static readonly Dictionary<string, (string PromiseNumber, Guid CitizenId, DateTime ExpiryDate)> _tokens = new();

    public MockMObywatelGateway(ILogger<MockMObywatelGateway> logger)
    {
        _logger = logger;
    }

    public Task<MObywatelQrResult> GenerateQrTokenAsync(string promiseNumber, Guid citizenId)
    {
        _logger.LogInformation("[MOCK] mObywatel QR token generation requested for promise: {PromiseNumber}", promiseNumber);

        // Generate mock QR token
        var qrToken = $"QR-{Guid.NewGuid():N}"[..24].ToUpper();

        // Store for later verification
        _tokens[qrToken] = (promiseNumber, citizenId, DateTime.UtcNow.AddMonths(3));

        _logger.LogInformation("[MOCK] Generated QR token: {QrToken} for promise: {PromiseNumber}", qrToken, promiseNumber);

        return Task.FromResult(new MObywatelQrResult
        {
            Success = true,
            QrToken = qrToken
        });
    }

    public Task<MObywatelVerifyResult> VerifyQrTokenAsync(string qrToken)
    {
        _logger.LogInformation("[MOCK] mObywatel QR token verification requested: {QrToken}", qrToken);

        if (_tokens.TryGetValue(qrToken, out var tokenData))
        {
            return Task.FromResult(new MObywatelVerifyResult
            {
                IsValid = tokenData.ExpiryDate >= DateTime.UtcNow,
                PromiseNumber = tokenData.PromiseNumber,
                CitizenId = tokenData.CitizenId,
                ExpiryDate = tokenData.ExpiryDate
            });
        }

        return Task.FromResult(new MObywatelVerifyResult
        {
            IsValid = false,
            ErrorMessage = "QR token not found"
        });
    }
}
