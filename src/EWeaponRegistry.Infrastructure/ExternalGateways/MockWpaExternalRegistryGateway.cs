using EWeaponRegistry.Application.Interfaces.ExternalGateways;
using Microsoft.Extensions.Logging;

namespace EWeaponRegistry.Infrastructure.ExternalGateways;

/// <summary>
/// Mock implementation of WPA External Registry Gateway.
/// NOTE: This is a DEMONSTRATION ONLY.
/// Real integration with WPA/Police systems would require:
/// - Secure VPN or dedicated network connection
/// - Formal agreements and data protection procedures
/// - Access credentials and certificates
/// - Audit logging of all queries
/// </summary>
public class MockWpaExternalRegistryGateway : IWpaExternalRegistryGateway
{
    private readonly ILogger<MockWpaExternalRegistryGateway> _logger;

    public MockWpaExternalRegistryGateway(ILogger<MockWpaExternalRegistryGateway> logger)
    {
        _logger = logger;
    }

    public Task<WeaponBookVerifyResult> VerifyWeaponBookNumberAsync(string weaponBookNumber, string pesel)
    {
        _logger.LogInformation("[MOCK] WPA Registry verification for weapon book: {WeaponBookNumber}", weaponBookNumber);

        // Simulate verification
        if (string.IsNullOrEmpty(weaponBookNumber) || weaponBookNumber.Length < 5)
        {
            return Task.FromResult(new WeaponBookVerifyResult
            {
                IsValid = false,
                ErrorMessage = "Invalid weapon book number format"
            });
        }

        // Mock successful verification
        return Task.FromResult(new WeaponBookVerifyResult
        {
            IsValid = true,
            OwnerName = "Jan Kowalski",
            Status = "Active",
            IssueDate = DateTime.UtcNow.AddYears(-2)
        });
    }

    public Task<PermitVerifyResult> VerifyPermitAsync(string permitNumber, string pesel)
    {
        _logger.LogInformation("[MOCK] WPA Registry verification for permit: {PermitNumber}", permitNumber);

        // Simulate verification
        if (string.IsNullOrEmpty(permitNumber) || permitNumber.Length < 5)
        {
            return Task.FromResult(new PermitVerifyResult
            {
                IsValid = false,
                ErrorMessage = "Invalid permit number format"
            });
        }

        // Mock successful verification
        return Task.FromResult(new PermitVerifyResult
        {
            IsValid = true,
            PermitType = "Sport",
            Status = "Active",
            ExpiryDate = DateTime.UtcNow.AddYears(3),
            MaxFirearms = 5
        });
    }
}
