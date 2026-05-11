using EWeaponRegistry.Application.DTOs.Admin;
using EWeaponRegistry.Application.DTOs.Common;
using EWeaponRegistry.Domain.Enums;

namespace EWeaponRegistry.Application.Interfaces;

public interface IAdminService
{
    Task<PaginatedResult<AdminUserDto>> GetUsersAsync(PaginationParams pagination);
    Task<AdminUserDto> CreateUserAsync(CreateUserRequest request);
    Task UpdateUserRoleAsync(Guid userId, UserRole newRole);
    Task UpdateUserStatusAsync(Guid userId, bool isActive);
    Task<PaginatedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogFilter filter, PaginationParams pagination);
    Task<Dictionary<string, List<string>>> GetDictionariesAsync();
}
