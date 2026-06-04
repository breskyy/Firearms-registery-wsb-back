namespace EWeaponRegistry.Application.DTOs.Wpa;

public class WpaPermitMedicalExamRenewalDto
{
    public Guid Id { get; set; }
    public Guid PermitId { get; set; }
    public string PermitNumber { get; set; } = string.Empty;
    public string PermitTypeName { get; set; } = string.Empty;
    public Guid CitizenId { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string CitizenPesel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public DateTime ProposedMedicalExamExpiryDate { get; set; }
    public DateTime ProposedPsychologicalExamExpiryDate { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public IList<WpaPermitMedicalExamRenewalAttachmentDto> Attachments { get; set; } =
        new List<WpaPermitMedicalExamRenewalAttachmentDto>();
}

public class WpaPermitMedicalExamRenewalAttachmentDto
{
    public Guid Id { get; set; }
    public string AttachmentType { get; set; } = string.Empty;
    public string AttachmentTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApprovePermitMedicalExamRenewalRequest
{
    public DateTime? MedicalExamExpiryDate { get; set; }
    public DateTime? PsychologicalExamExpiryDate { get; set; }
}

public class RejectPermitMedicalExamRenewalRequest
{
    public string Reason { get; set; } = string.Empty;
}
