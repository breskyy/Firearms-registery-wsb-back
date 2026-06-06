using EWeaponRegistry.Domain.Common;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Domain.Entities;

public class PromiseApplicationAttachment : BaseEntity
{
    public Guid PromiseApplicationId { get; set; }
    public PromiseApplicationAttachmentType AttachmentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public byte[] Content { get; set; } = [];

    public PromiseApplication PromiseApplication { get; set; } = null!;
}
