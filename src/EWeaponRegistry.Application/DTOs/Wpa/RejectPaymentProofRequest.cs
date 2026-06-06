using System.ComponentModel.DataAnnotations;

namespace EWeaponRegistry.Application.DTOs.Wpa;

public class RejectPaymentProofRequest
{
    [Required]
    [MinLength(10)]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
}
