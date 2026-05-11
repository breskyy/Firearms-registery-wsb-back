namespace EWeaponRegistry.Application.Interfaces.ExternalGateways;

/// <summary>
/// Gateway for mObywatel integration.
/// NOTE: This is a MOCK interface. Real integration would require:
/// - Access to mObywatel API
/// - Proper authentication and authorization
/// - Handling of digital documents
/// - HTTPS/TLS communication
/// </summary>
public interface IMObywatelGateway
{
    Task<MObywatelQrResult> GenerateQrTokenAsync(string promiseNumber, Guid citizenId);
    Task<MObywatelVerifyResult> VerifyQrTokenAsync(string qrToken);
}

public class MObywatelQrResult
{
    public bool Success { get; set; }
    public string? QrToken { get; set; }
    public string? ErrorMessage { get; set; }
}

public class MObywatelVerifyResult
{
    public bool IsValid { get; set; }
    public string? PromiseNumber { get; set; }
    public Guid? CitizenId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ErrorMessage { get; set; }
}
