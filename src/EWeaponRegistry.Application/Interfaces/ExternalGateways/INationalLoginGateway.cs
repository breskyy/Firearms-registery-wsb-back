namespace EWeaponRegistry.Application.Interfaces.ExternalGateways;

/// <summary>
/// Gateway for National Login (login.gov.pl / Węzeł Krajowy) integration.
/// NOTE: This is a MOCK interface. Real integration would require:
/// - Access to test/production environments
/// - OAuth2/OpenID Connect implementation
/// - Certificates and formal approval
/// - HTTPS/TLS communication
/// </summary>
public interface INationalLoginGateway
{
    Task<NationalLoginVerifyResult> VerifyIdentityAsync(string token);
}

public class NationalLoginVerifyResult
{
    public bool IsValid { get; set; }
    public string? Pesel { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ErrorMessage { get; set; }
}
