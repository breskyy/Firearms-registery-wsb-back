using EWeaponRegistry.Application.DTOs.Common;
using EWeaponRegistry.Application.DTOs.Wpa;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Application.Interfaces;

public interface IWpaService
{
    Task<PaginatedResult<WpaCitizenDto>> GetCitizensAsync(PaginationParams pagination);
    Task<WpaCitizenDto?> GetCitizenByIdAsync(Guid citizenId);

    Task<PaginatedResult<WpaFirearmSearchResult>> SearchFirearmsAsync(
        string? serialNumber,
        string? pesel,
        string? permitNumber,
        PermitType? permitType,
        PaginationParams pagination);

    Task<PaginatedResult<WpaPromiseApplicationDto>> GetPromiseApplicationsAsync(
        PromiseApplicationStatus? status,
        PaginationParams pagination);

    Task<WpaPromiseApplicationDto?> GetPromiseApplicationByIdAsync(Guid applicationId);
    Task MarkApplicationUnderReviewAsync(Guid officerId, Guid applicationId);
    Task ApproveApplicationAsync(Guid officerId, Guid applicationId);
    Task RejectApplicationAsync(Guid officerId, Guid applicationId, string? reason);
    Task RequireCorrectionAsync(Guid officerId, Guid applicationId, string? notes);

    Task<PaginatedResult<MedicalAlertDto>> GetMedicalAlertsAsync(bool? resolved, PaginationParams pagination);

    Task<PaginatedResult<WpaPermitApplicationDto>> GetPermitApplicationsAsync(
        PermitApplicationStatus? status,
        PaginationParams pagination);

    Task<WpaPermitApplicationDto?> GetPermitApplicationByIdAsync(Guid applicationId);
    Task MarkPermitApplicationUnderReviewAsync(Guid officerId, Guid applicationId);
    Task ApprovePermitApplicationAsync(Guid officerId, Guid applicationId, ApprovePermitApplicationRequest request);
    Task RejectPermitApplicationAsync(Guid officerId, Guid applicationId, string? reason);
    Task RequirePermitApplicationCorrectionAsync(Guid officerId, Guid applicationId, string? notes);
    Task VerifyPermitApplicationPaymentAsync(Guid officerId, Guid applicationId);
    Task VerifyPromiseApplicationPaymentAsync(Guid officerId, Guid applicationId);
    Task RejectPermitApplicationPaymentProofAsync(Guid officerId, Guid applicationId, string comment);
    Task RejectPromiseApplicationPaymentProofAsync(Guid officerId, Guid applicationId, string comment);

    Task SuspendPermitAsync(Guid officerId, Guid permitId, string? reason);
    Task RevokePermitAsync(Guid officerId, Guid permitId, string? reason);
    Task RestorePermitAsync(Guid officerId, Guid permitId, string? reason);
    Task UpdatePermitMedicalExamsAsync(Guid officerId, Guid permitId, UpdatePermitMedicalExamsRequest request);
}
