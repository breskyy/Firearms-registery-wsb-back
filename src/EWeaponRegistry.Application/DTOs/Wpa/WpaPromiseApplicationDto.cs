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
}

public class ReviewPromiseApplicationRequest
{
    [MaxLength(1000)]
    public string? Reason { get; set; }
}
