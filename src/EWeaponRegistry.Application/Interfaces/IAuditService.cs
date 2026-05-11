namespace EWeaponRegistry.Application.Interfaces;

/// <summary>
/// Service for logging audit events.
/// All critical operations must be logged.
/// </summary>
public interface IAuditService
{
    Task LogAsync(
        string action,
        string? entityType = null,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? description = null);

    Task LogLoginAsync(Guid userId, bool success, string? ipAddress = null);
    Task LogAccessDeniedAsync(string resource, string? reason = null);
}
