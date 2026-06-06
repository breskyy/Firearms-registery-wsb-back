using EWeaponRegistry.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EWeaponRegistry.Application.DTOs.Citizen;

public class PromiseApplicationDto
{
    public Guid Id { get; set; }
    public Guid PermitId { get; set; }
    public string PermitNumber { get; set; } = string.Empty;
    public string RequestedWeaponType { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public PromiseApplicationStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? RejectionReason { get; set; }
    public string? CorrectionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public decimal FeeAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusName => PaymentStatus.ToString();
    public IList<PromiseApplicationAttachmentDto> Attachments { get; set; } = new List<PromiseApplicationAttachmentDto>();
}

public class CreatePromiseApplicationRequest
{
    [Required]
    public Guid PermitId { get; set; }

    [Required]
    [MaxLength(100)]
    public string RequestedWeaponType { get; set; } = string.Empty;

    [Required]
    [Range(1, 10)]
    public int RequestedQuantity { get; set; }
}

public class UpdatePromiseApplicationCorrectionRequest
{
    [Required]
    public Guid PermitId { get; set; }

    [Required]
    [MaxLength(100)]
    public string RequestedWeaponType { get; set; } = string.Empty;

    [Required]
    [Range(1, 10)]
    public int RequestedQuantity { get; set; }
}
