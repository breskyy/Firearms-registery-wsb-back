using EWeaponRegistry.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EWeaponRegistry.Application.DTOs.Wpa;

public class WpaPromiseApplicationDto
{
    public Guid Id { get; set; }
    public Guid CitizenId { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string CitizenPesel { get; set; } = string.Empty;
    public Guid PermitId { get; set; }
    public string PermitNumber { get; set; } = string.Empty;
    public string PermitType { get; set; } = string.Empty;
    public string RequestedWeaponType { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public PromiseApplicationStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? RejectionReason { get; set; }
    public string? CorrectionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByOfficerName { get; set; }
    public decimal FeeAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusName => PaymentStatus.ToString();
    public PaymentMethod? PaymentMethod { get; set; }
    public string? PaymentMethodName => PaymentMethod?.ToString();
    public string? PaymentReferenceId { get; set; }
    public IList<WpaPromiseApplicationAttachmentDto> Attachments { get; set; } = new List<WpaPromiseApplicationAttachmentDto>();
}

public class WpaPromiseApplicationAttachmentDto
{
    public Guid Id { get; set; }
    public string AttachmentType { get; set; } = string.Empty;
    public string AttachmentTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewPromiseApplicationRequest
{
    [MaxLength(1000)]
    public string? Reason { get; set; }
}
