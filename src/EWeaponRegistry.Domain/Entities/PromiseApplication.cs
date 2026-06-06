using EWeaponRegistry.Domain.Common;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Domain.Entities;

/// <summary>
/// Application for e-promise - submitted by citizen, reviewed by WPA officer.
/// </summary>
public class PromiseApplication : BaseEntity
{
    public Guid CitizenId { get; set; }
    public Guid PermitId { get; set; }
    public string RequestedWeaponType { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public PromiseApplicationStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public string? CorrectionNotes { get; set; }
    public Guid? ReviewedByOfficerId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Generated promise (after approval)
    public Guid? GeneratedPromiseId { get; set; }
    public decimal FeeAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? PaymentReferenceId { get; set; }
    public string? PaymentRejectionComment { get; set; }

    // Navigation properties
    public CitizenProfile Citizen { get; set; } = null!;
    public Permit Permit { get; set; } = null!;
    public User? ReviewedByOfficer { get; set; }
    public Promise? GeneratedPromise { get; set; }
    public ICollection<PromiseApplicationAttachment> Attachments { get; set; } = new List<PromiseApplicationAttachment>();
}
