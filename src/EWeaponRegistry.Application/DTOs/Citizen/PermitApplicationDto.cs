using EWeaponRegistry.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EWeaponRegistry.Application.DTOs.Citizen;

public class PermitApplicationDto
{
    public Guid Id { get; set; }
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
    public decimal FeeAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusName => PaymentStatus.ToString();
    public IList<PermitApplicationAttachmentDto> Attachments { get; set; } = new List<PermitApplicationAttachmentDto>();
}

public class PermitApplicationAttachmentDto
{
    public Guid Id { get; set; }
    public string AttachmentType { get; set; } = string.Empty;
    public string AttachmentTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePermitApplicationRequest
{
    [Required]
    public PermitType RequestedPermitType { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;

    public DateTime? MedicalExamExpiryDate { get; set; }

    public DateTime? PsychologicalExamExpiryDate { get; set; }
}

public class UpdatePermitApplicationCorrectionRequest
{
    [Required]
    public PermitType RequestedPermitType { get; set; }

    [Required]
    [MinLength(20)]
    [MaxLength(2000)]
    public string Reason { get; set; } = string.Empty;

    public DateTime? MedicalExamExpiryDate { get; set; }

    public DateTime? PsychologicalExamExpiryDate { get; set; }
}
