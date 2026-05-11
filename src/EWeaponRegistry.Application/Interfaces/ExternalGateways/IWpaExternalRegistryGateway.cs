namespace EWeaponRegistry.Application.Interfaces.ExternalGateways;

/// <summary>
/// Gateway for WPA/Police external registry integration.
/// NOTE: This is a MOCK interface. Real integration would require:
/// - Access to WPA/Police information systems
/// - Secure VPN or dedicated network connection
/// - Formal agreements and data protection procedures
/// - Audit logging of all queries
/// </summary>
public interface IWpaExternalRegistryGateway
{
    Task<WeaponBookVerifyResult> VerifyWeaponBookNumberAsync(string weaponBookNumber, string pesel);
    Task<PermitVerifyResult> VerifyPermitAsync(string permitNumber, string pesel);
}

public class WeaponBookVerifyResult
{
    public bool IsValid { get; set; }
    public string? OwnerName { get; set; }
    public string? Status { get; set; }  // Active, Suspended, Revoked
    public DateTime? IssueDate { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PermitVerifyResult
{
    public bool IsValid { get; set; }
    public string? PermitType { get; set; }
    public string? Status { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? MaxFirearms { get; set; }
    public string? ErrorMessage { get; set; }
}
