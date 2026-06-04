using EWeaponRegistry.Domain.Common;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Domain.Entities;

public class PermitMedicalExamRenewalAttachment : BaseEntity
{
    public Guid PermitMedicalExamRenewalId { get; set; }
    public PermitApplicationAttachmentType AttachmentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();

    public PermitMedicalExamRenewal Renewal { get; set; } = null!;
}
