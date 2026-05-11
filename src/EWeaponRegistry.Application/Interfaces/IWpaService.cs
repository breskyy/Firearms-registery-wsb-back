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
}
