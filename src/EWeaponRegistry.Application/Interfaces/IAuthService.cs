using EWeaponRegistry.Application.DTOs.Auth;

namespace EWeaponRegistry.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null);
    Task<UserDto> GetCurrentUserAsync(Guid userId);
}
