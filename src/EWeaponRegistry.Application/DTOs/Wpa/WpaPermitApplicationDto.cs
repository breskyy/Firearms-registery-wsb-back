using EWeaponRegistry.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EWeaponRegistry.Application.DTOs.Wpa;

public class WpaPermitApplicationDto
{
    public Guid Id { get; set; }
    public Guid CitizenId { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string CitizenPesel { get; set; } = string.Empty;
    public PermitType RequestedPermitType { get; set; }
    public string RequestedPermitTypeName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime? MedicalExamExpiryDate { get; set; }
    public DateTime? PsychologicalExamExpiryDate { get; set; }
    public PermitApplicationStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? RejectionReason { get; set; }
    public string? CorrectionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByOfficerName { get; set; }
    public decimal FeeAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusName => PaymentStatus.ToString();
    public IList<WpaPermitApplicationAttachmentDto> Attachments { get; set; } = new List<WpaPermitApplicationAttachmentDto>();
}

public class WpaPermitApplicationAttachmentDto
{
    public Guid Id { get; set; }
    public string AttachmentType { get; set; } = string.Empty;
    public string AttachmentTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApprovePermitApplicationRequest
{
    [Required]
    [Range(1, 50)]
    public int MaxFirearms { get; set; }

    [Required]
    public DateTime? MedicalExamExpiryDate { get; set; }

    [Required]
    public DateTime? PsychologicalExamExpiryDate { get; set; }
}

public class ReviewPermitApplicationRequest
{
    [MaxLength(1000)]
    public string? Reason { get; set; }
}

public class ManagePermitRequest
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}

public class UpdatePermitMedicalExamsRequest
{
    [Required]
    public DateTime MedicalExamExpiryDate { get; set; }

    [Required]
    public DateTime PsychologicalExamExpiryDate { get; set; }
}
