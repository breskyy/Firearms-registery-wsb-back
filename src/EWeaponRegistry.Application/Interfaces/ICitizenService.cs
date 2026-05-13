using EWeaponRegistry.Application.DTOs.Citizen;
using EWeaponRegistry.Application.DTOs.Common;

namespace EWeaponRegistry.Application.Interfaces;

public interface ICitizenService
{
    Task<CitizenProfileDto> GetMyProfileAsync(Guid userId);
    Task<IList<PermitDto>> GetMyPermitsAsync(Guid userId);
    Task<IList<FirearmDto>> GetMyFirearmsAsync(Guid userId);
    Task<FirearmDetailDto?> GetMyFirearmByIdAsync(Guid userId, Guid firearmId);
    Task ReportFirearmLostAsync(Guid userId, Guid firearmId, ReportFirearmLostRequest request);
    Task<IList<PromiseDto>> GetMyPromisesAsync(Guid userId);
    Task<IList<PromiseApplicationDto>> GetMyPromiseApplicationsAsync(Guid userId);
    Task<PromiseApplicationDto> CreatePromiseApplicationAsync(Guid userId, CreatePromiseApplicationRequest request);
    Task<PromiseApplicationDto> UpdatePromiseApplicationCorrectionAsync(Guid userId, Guid applicationId, UpdatePromiseApplicationCorrectionRequest request);
    Task<IList<TransferRequestDto>> GetMyTransferRequestsAsync(Guid userId);
    Task<TransferRequestDto> CreateTransferRequestAsync(Guid userId, CreateTransferRequest request);
    Task AcceptTransferRequestAsync(Guid userId, Guid transferRequestId);
    Task RejectTransferRequestAsync(Guid userId, Guid transferRequestId);
    Task CancelTransferRequestAsync(Guid userId, Guid transferRequestId);
    Task<IList<PermitApplicationDto>> GetMyPermitApplicationsAsync(Guid userId);
    Task<PermitApplicationDto> CreatePermitApplicationAsync(Guid userId, CreatePermitApplicationRequest request);
    Task<PermitApplicationDto> UpdatePermitApplicationCorrectionAsync(Guid userId, Guid applicationId, UpdatePermitApplicationCorrectionRequest request);
    Task<IList<CitizenMedicalAlertDto>> GetMyMedicalAlertsAsync(Guid userId);
}
