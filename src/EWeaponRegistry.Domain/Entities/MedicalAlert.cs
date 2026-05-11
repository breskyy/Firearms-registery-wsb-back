using EWeaponRegistry.Domain.Common;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Domain.Entities;

/// <summary>
/// Medical/psychological exam expiration alerts.
/// </summary>
public class MedicalAlert : BaseEntity
{
    public Guid CitizenId { get; set; }
    public Guid? PermitId { get; set; }
    public MedicalAlertType AlertType { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public bool IsResolved { get; set; }

    // Navigation properties
    public CitizenProfile Citizen { get; set; } = null!;
    public Permit? Permit { get; set; }
}
