using EWeaponRegistry.Application.DTOs.Citizen;
using EWeaponRegistry.Application.DTOs.Common;
using EWeaponRegistry.Application.DTOs.Wpa;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Application.Interfaces;

public record RenewalCertificateUpload(string FileName, string ContentType, byte[] Content);

public interface IPermitMedicalExamRenewalService
{
    Task<PermitMedicalExamRenewalDto> SubmitRenewalAsync(
        Guid userId,
        Guid permitId,
        SubmitPermitMedicalExamRenewalRequest request,
        RenewalCertificateUpload medicalCertificate,
        RenewalCertificateUpload psychologicalCertificate);

    Task<IList<PermitMedicalExamRenewalDto>> GetMyRenewalsForPermitAsync(Guid userId, Guid permitId);
    Task<IList<PermitMedicalExamRenewalDto>> GetMyRenewalsAsync(Guid userId);

    Task<PaginatedResult<WpaPermitMedicalExamRenewalDto>> GetRenewalsForWpaAsync(
        PermitMedicalExamRenewalStatus? status,
        PaginationParams pagination);

    Task<WpaPermitMedicalExamRenewalDto?> GetRenewalByIdForWpaAsync(Guid renewalId);
    Task MarkRenewalUnderReviewAsync(Guid officerUserId, Guid renewalId);
    Task ApproveRenewalAsync(Guid officerUserId, Guid renewalId, ApprovePermitMedicalExamRenewalRequest request);
    Task RejectRenewalAsync(Guid officerUserId, Guid renewalId, RejectPermitMedicalExamRenewalRequest request);

    Task<(byte[] Content, string ContentType, string FileName)?> GetRenewalAttachmentAsync(
        Guid renewalId,
        Guid attachmentId,
        bool requireOfficer);
}
