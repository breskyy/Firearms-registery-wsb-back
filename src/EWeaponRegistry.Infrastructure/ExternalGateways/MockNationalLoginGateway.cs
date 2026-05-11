using EWeaponRegistry.Application.Interfaces.ExternalGateways;
using Microsoft.Extensions.Logging;

namespace EWeaponRegistry.Infrastructure.ExternalGateways;

/// <summary>
/// Mock implementation of National Login Gateway.
/// NOTE: This is a DEMONSTRATION ONLY.
/// Real integration with login.gov.pl / Węzeł Krajowy would require:
/// - OAuth2/OpenID Connect implementation
/// - Certificates and formal approval
/// - Access to test/production environments
/// - HTTPS/TLS communication
/// </summary>
public class MockNationalLoginGateway : INationalLoginGateway
{
    private readonly ILogger<MockNationalLoginGateway> _logger;

    public MockNationalLoginGateway(ILogger<MockNationalLoginGateway> logger)
    {
        _logger = logger;
    }

    public Task<NationalLoginVerifyResult> VerifyIdentityAsync(string token)
    {
        _logger.LogInformation("[MOCK] National Login verification requested for token: {Token}", token[..Math.Min(10, token.Length)] + "...");

        // Simulate verification - in real implementation this would call external API
        if (string.IsNullOrEmpty(token) || token.Length < 10)
        {
            return Task.FromResult(new NationalLoginVerifyResult
            {
                IsValid = false,
                ErrorMessage = "Invalid token format"
            });
        }

        // Mock successful verification
        return Task.FromResult(new NationalLoginVerifyResult
        {
            IsValid = true,
            Pesel = "90010112345",
            FirstName = "Jan",
            LastName = "Kowalski"
        });
    }
}
