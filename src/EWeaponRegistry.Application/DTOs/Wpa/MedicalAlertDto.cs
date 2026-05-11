using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Application.DTOs.Wpa;

public class MedicalAlertDto
{
    public Guid Id { get; set; }
    public Guid CitizenId { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string CitizenPesel { get; set; } = string.Empty;
    public Guid? PermitId { get; set; }
    public string? PermitNumber { get; set; }
    public MedicalAlertType AlertType { get; set; }
    public string AlertTypeName => AlertType.ToString();
    public string Message { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; }
}
