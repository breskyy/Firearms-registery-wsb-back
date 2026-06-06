using EWeaponRegistry.Domain.Common;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Domain.Entities;

public class PermitApplication : BaseEntity
{
    public Guid CitizenId { get; set; }
    public PermitType RequestedPermitType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public PermitApplicationStatus Status { get; set; }
    public string? MedicalExamExpiryDateEncrypted { get; set; }
    public string? PsychologicalExamExpiryDateEncrypted { get; set; }
    public string? RejectionReason { get; set; }
    public string? CorrectionNotes { get; set; }
    public Guid? ReviewedByOfficerId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? GeneratedPermitId { get; set; }
    public decimal FeeAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? PaymentReferenceId { get; set; }

    public CitizenProfile Citizen { get; set; } = null!;
    public User? ReviewedByOfficer { get; set; }
    public Permit? GeneratedPermit { get; set; }
    public ICollection<PermitApplicationAttachment> Attachments { get; set; } = new List<PermitApplicationAttachment>();
}
