using EWeaponRegistry.Domain.Common;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Domain.Entities;

public class Permit : BaseEntity
{
    public Guid CitizenId { get; set; }
    public string PermitNumber { get; set; } = string.Empty;
    public PermitType PermitType { get; set; }
    public PermitStatus Status { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int MaxFirearms { get; set; }
    public int UsedSlots { get; set; }

    // Medical examination dates (encrypted for privacy)
    public string? MedicalExamExpiryDateEncrypted { get; set; }
    public string? PsychologicalExamExpiryDateEncrypted { get; set; }

    // Navigation properties
    public CitizenProfile Citizen { get; set; } = null!;
    public ICollection<Promise> Promises { get; set; } = new List<Promise>();
    public ICollection<PromiseApplication> PromiseApplications { get; set; } = new List<PromiseApplication>();
    public ICollection<MedicalAlert> MedicalAlerts { get; set; } = new List<MedicalAlert>();
}
