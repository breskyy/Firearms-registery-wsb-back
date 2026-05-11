namespace EWeaponRegistry.Application.DTOs.Shop;

public class VerifyPermitRequest
{
    public string? QrToken { get; set; }
    public string? PromiseNumber { get; set; }
}

public class VerifyPermitResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? CitizenName { get; set; }
    public string? PermitNumber { get; set; }
    public string? PermitType { get; set; }
    public int AvailableSlots { get; set; }
    public string? WeaponType { get; set; }
    public int RemainingPromiseQuantity { get; set; }
    public DateTime? PromiseExpiryDate { get; set; }
    public bool MedicalExamsValid { get; set; }
}
