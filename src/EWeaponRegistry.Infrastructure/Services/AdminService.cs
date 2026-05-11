using EWeaponRegistry.Application.DTOs.Admin;
using EWeaponRegistry.Application.DTOs.Common;
using EWeaponRegistry.Application.Exceptions;
using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Domain.Entities;
using EWeaponRegistry.Domain.Enums;
using EWeaponRegistry.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EWeaponRegistry.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;

    public AdminService(AppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<PaginatedResult<AdminUserDto>> GetUsersAsync(PaginationParams pagination)
    {
        var query = _context.Users.AsQueryable();

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.Email)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var items = users.Select(u => new AdminUserDto
        {
            Id = u.Id,
            Email = u.Email,
            Role = u.Role,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        }).ToList();

        return new PaginatedResult<AdminUserDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<AdminUserDto> CreateUserAsync(CreateUserRequest request)
    {
        // Check if email already exists
        var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existing != null)
        {
            throw new ConflictException($"User with email {request.Email} already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("AdminCreateUser", "User", user.Id.ToString(),
            newValues: new { user.Email, user.Role });

        return new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task UpdateUserRoleAsync(Guid userId, UserRole newRole)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var oldRole = user.Role;
        user.Role = newRole;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("AdminUpdateUserRole", "User", userId.ToString(),
            oldValues: new { Role = oldRole },
            newValues: new { Role = newRole });
    }

    public async Task UpdateUserStatusAsync(Guid userId, bool isActive)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var oldStatus = user.IsActive;
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("AdminUpdateUserStatus", "User", userId.ToString(),
            oldValues: new { IsActive = oldStatus },
            newValues: new { IsActive = isActive });
    }

    public async Task<PaginatedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogFilter filter, PaginationParams pagination)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (filter.UserId.HasValue)
        {
            query = query.Where(al => al.UserId == filter.UserId);
        }

        if (!string.IsNullOrEmpty(filter.Action))
        {
            query = query.Where(al => al.Action.Contains(filter.Action));
        }

        if (!string.IsNullOrEmpty(filter.EntityType))
        {
            query = query.Where(al => al.EntityType == filter.EntityType);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(al => al.TimestampUtc >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(al => al.TimestampUtc <= filter.ToDate.Value);
        }

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(al => al.TimestampUtc)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        // Get user emails for display
        var userIds = logs.Where(l => l.UserId.HasValue).Select(l => l.UserId!.Value).Distinct().ToList();
        var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Email);

        var items = logs.Select(al => new AuditLogDto
        {
            Id = al.Id,
            UserId = al.UserId,
            UserEmail = al.UserId.HasValue && users.TryGetValue(al.UserId.Value, out var email) ? email : null,
            UserRole = al.UserRole,
            Action = al.Action,
            EntityType = al.EntityType,
            EntityId = al.EntityId,
            TimestampUtc = al.TimestampUtc,
            IpAddress = al.IpAddress,
            OldValuesJson = al.OldValuesJson,
            NewValuesJson = al.NewValuesJson,
            Description = al.Description
        }).ToList();

        return new PaginatedResult<AuditLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public Task<Dictionary<string, List<string>>> GetDictionariesAsync()
    {
        var dictionaries = new Dictionary<string, List<string>>
        {
            ["UserRoles"] = Enum.GetNames<UserRole>().ToList(),
            ["PermitTypes"] = Enum.GetNames<PermitType>().ToList(),
            ["PermitStatuses"] = Enum.GetNames<PermitStatus>().ToList(),
            ["FirearmStatuses"] = Enum.GetNames<FirearmStatus>().ToList(),
            ["FirearmCategories"] = Enum.GetNames<FirearmCategory>().ToList(),
            ["TransferTypes"] = Enum.GetNames<TransferType>().ToList(),
            ["TransferRequestStatuses"] = Enum.GetNames<TransferRequestStatus>().ToList(),
            ["PromiseStatuses"] = Enum.GetNames<PromiseStatus>().ToList(),
            ["PromiseApplicationStatuses"] = Enum.GetNames<PromiseApplicationStatus>().ToList(),
            ["PaymentStatuses"] = Enum.GetNames<PaymentStatus>().ToList(),
            ["MedicalAlertTypes"] = Enum.GetNames<MedicalAlertType>().ToList()
        };

        return Task.FromResult(dictionaries);
    }
}
