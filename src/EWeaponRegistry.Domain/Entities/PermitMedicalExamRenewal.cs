using EWeaponRegistry.Domain.Common;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Domain.Entities;

public class PermitMedicalExamRenewal : BaseEntity
{
    public Guid PermitId { get; set; }
    public Guid CitizenId { get; set; }
    public PermitMedicalExamRenewalStatus Status { get; set; }
    public string ProposedMedicalExpiryDateEncrypted { get; set; } = string.Empty;
    public string ProposedPsychologicalExpiryDateEncrypted { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByOfficerUserId { get; set; }

    public Permit Permit { get; set; } = null!;
    public CitizenProfile Citizen { get; set; } = null!;
    public ICollection<PermitMedicalExamRenewalAttachment> Attachments { get; set; } =
        new List<PermitMedicalExamRenewalAttachment>();
}
