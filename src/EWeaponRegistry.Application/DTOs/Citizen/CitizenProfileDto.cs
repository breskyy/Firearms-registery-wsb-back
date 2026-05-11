namespace EWeaponRegistry.Application.DTOs.Citizen;

public class CitizenProfileDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PeselMasked { get; set; } = string.Empty;  // Show only last 4 digits
    public string Address { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string WeaponBookNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CitizenProfileDetailDto : CitizenProfileDto
{
    public string Pesel { get; set; } = string.Empty;  // Full PESEL (only for authorized access)
}
