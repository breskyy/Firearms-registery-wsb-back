using EWeaponRegistry.Domain.Common;

namespace EWeaponRegistry.Domain.Entities;

public class CitizenProfile : BaseEntity
{
    public Guid UserId { get; set; }

    // Encrypted sensitive data (AES-256)
    public string FirstNameEncrypted { get; set; } = string.Empty;
    public string LastNameEncrypted { get; set; } = string.Empty;
    public string PeselEncrypted { get; set; } = string.Empty;
    public string AddressEncrypted { get; set; } = string.Empty;
    public string DocumentNumberEncrypted { get; set; } = string.Empty;
    public string WeaponBookNumberEncrypted { get; set; } = string.Empty;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Permit> Permits { get; set; } = new List<Permit>();
    public ICollection<Firearm> Firearms { get; set; } = new List<Firearm>();
    public ICollection<Promise> Promises { get; set; } = new List<Promise>();
    public ICollection<PromiseApplication> PromiseApplications { get; set; } = new List<PromiseApplication>();
    public ICollection<TransferRequest> TransferRequestsAsSeller { get; set; } = new List<TransferRequest>();
    public ICollection<TransferRequest> TransferRequestsAsBuyer { get; set; } = new List<TransferRequest>();
    public ICollection<MedicalAlert> MedicalAlerts { get; set; } = new List<MedicalAlert>();
    public ICollection<PermitApplication> PermitApplications { get; set; } = new List<PermitApplication>();
}
