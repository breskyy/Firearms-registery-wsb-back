using System.Text.Json;
using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Domain.Entities;
using EWeaponRegistry.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EWeaponRegistry.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(
        string action,
        string? entityType = null,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? description = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        Guid? userId = null;
        string? userRole = null;

        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = httpContext.User.FindFirst("sub") ?? httpContext.User.FindFirst("userId");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var roleClaim = httpContext.User.FindFirst("role") ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role);
            userRole = roleClaim?.Value;
        }

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserRole = userRole,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            TimestampUtc = DateTime.UtcNow,
            IpAddress = GetClientIpAddress(),
            OldValuesJson = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValuesJson = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            Description = description
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Audit: {Action} by User {UserId} ({Role}) on {EntityType}/{EntityId}",
            action, userId, userRole, entityType, entityId);
    }

    public async Task LogLoginAsync(Guid userId, bool success, string? ipAddress = null)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = success ? "Login.Success" : "Login.Failed",
            TimestampUtc = DateTime.UtcNow,
            IpAddress = ipAddress ?? GetClientIpAddress(),
            Description = success ? "User logged in successfully" : "Login attempt failed"
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task LogAccessDeniedAsync(string resource, string? reason = null)
    {
        await LogAsync(
            "AccessDenied",
            entityType: "Resource",
            entityId: resource,
            description: reason ?? $"Access denied to {resource}");
    }

    private string? GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        // Check for forwarded headers (behind proxy/load balancer)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
