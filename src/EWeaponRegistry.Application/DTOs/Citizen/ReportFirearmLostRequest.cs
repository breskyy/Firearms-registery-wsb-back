using System.ComponentModel.DataAnnotations;

namespace EWeaponRegistry.Application.DTOs.Citizen;

public class ReportFirearmLostRequest
{
    [MaxLength(500)]
    public string? Description { get; set; }
}
