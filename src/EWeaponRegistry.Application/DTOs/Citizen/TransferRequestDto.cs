using EWeaponRegistry.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EWeaponRegistry.Application.DTOs.Citizen;

public class TransferRequestDto
{
    public Guid Id { get; set; }
    public Guid FirearmId { get; set; }
    public string FirearmDescription { get; set; } = string.Empty;
    public string? BuyerName { get; set; }
    public TransferType TransferType { get; set; }
    public string TransferTypeName => TransferType.ToString();
    public TransferRequestStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime? TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsSeller { get; set; }
    public bool IsBuyer { get; set; }
}

public class CreateTransferRequest
{
    [Required]
    public Guid FirearmId { get; set; }

    [Required]
    [StringLength(11, MinimumLength = 11)]
    public string BuyerPesel { get; set; } = string.Empty;

    [Required]
    public TransferType TransferType { get; set; }
}
