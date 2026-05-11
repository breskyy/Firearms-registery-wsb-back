using EWeaponRegistry.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EWeaponRegistry.Application.DTOs.Shop;

public class RegisterSaleRequest
{
    [Required]
    public string QrToken { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    public FirearmCategory Category { get; set; }

    [Required]
    [MaxLength(50)]
    public string Caliber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string SerialNumber { get; set; } = string.Empty;

    public int? ProductionYear { get; set; }
}

public class RegisterSaleResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? FirearmId { get; set; }
    public string? RegistrationNumber { get; set; }
}
