using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Application.DTOs.Citizen;

public class PermitDto
{
    public Guid Id { get; set; }
    public string PermitNumber { get; set; } = string.Empty;
    public PermitType PermitType { get; set; }
    public string PermitTypeName => PermitType.ToString();
    public PermitStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int MaxFirearms { get; set; }
    public int UsedSlots { get; set; }
    public int AvailableSlots => MaxFirearms - UsedSlots;
    public bool IsValid => Status == PermitStatus.Active && ExpiryDate >= DateTime.UtcNow.Date;
    public DateTime? MedicalExamExpiryDate { get; set; }
    public DateTime? PsychologicalExamExpiryDate { get; set; }
}
