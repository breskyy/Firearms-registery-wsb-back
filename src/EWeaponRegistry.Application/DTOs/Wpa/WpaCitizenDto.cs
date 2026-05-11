using EWeaponRegistry.Application.DTOs.Citizen;

namespace EWeaponRegistry.Application.DTOs.Wpa;

public class WpaCitizenDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Pesel { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string WeaponBookNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IList<PermitDto> Permits { get; set; } = new List<PermitDto>();
    public int TotalFirearms { get; set; }
    public int ActiveAlerts { get; set; }
}

public class WpaFirearmSearchResult
{
    public Guid Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Caliber { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerPesel { get; set; } = string.Empty;
    public string PermitNumber { get; set; } = string.Empty;
    public string PermitType { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
