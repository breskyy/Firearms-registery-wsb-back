namespace EWeaponRegistry.Application.DTOs.Citizen;

public class PermitMedicalExamRenewalDto
{
    public Guid Id { get; set; }
    public Guid PermitId { get; set; }
    public string PermitNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public DateTime ProposedMedicalExamExpiryDate { get; set; }
    public DateTime ProposedPsychologicalExamExpiryDate { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public IList<PermitMedicalExamRenewalAttachmentDto> Attachments { get; set; } =
        new List<PermitMedicalExamRenewalAttachmentDto>();
}

public class PermitMedicalExamRenewalAttachmentDto
{
    public Guid Id { get; set; }
    public string AttachmentType { get; set; } = string.Empty;
    public string AttachmentTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SubmitPermitMedicalExamRenewalRequest
{
    public DateTime MedicalExamExpiryDate { get; set; }
    public DateTime PsychologicalExamExpiryDate { get; set; }
}
