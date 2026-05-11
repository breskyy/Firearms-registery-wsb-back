using EWeaponRegistry.Application.DTOs.Admin;
using EWeaponRegistry.Application.DTOs.Common;
using EWeaponRegistry.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWeaponRegistry.Api.Controllers.V1;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Get all users (paginated)
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PaginatedResult<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<AdminUserDto>>> GetUsers([FromQuery] PaginationParams pagination)
    {
        var result = await _adminService.GetUsersAsync(pagination);
        return Ok(result);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost("users")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminUserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await _adminService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUsers), result);
    }

    /// <summary>
    /// Update user role
    /// </summary>
    [HttpPatch("users/{id:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequest request)
    {
        await _adminService.UpdateUserRoleAsync(id, request.Role);
        return NoContent();
    }

    /// <summary>
    /// Update user status (activate/deactivate)
    /// </summary>
    [HttpPatch("users/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        await _adminService.UpdateUserStatusAsync(id, request.IsActive);
        return NoContent();
    }

    /// <summary>
    /// Get audit logs (paginated, filterable)
    /// </summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(PaginatedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<AuditLogDto>>> GetAuditLogs(
        [FromQuery] AuditLogFilter filter,
        [FromQuery] PaginationParams pagination)
    {
        var result = await _adminService.GetAuditLogsAsync(filter, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Get system dictionaries (enums)
    /// </summary>
    [HttpGet("dictionaries")]
    [ProducesResponseType(typeof(Dictionary<string, List<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, List<string>>>> GetDictionaries()
    {
        var result = await _adminService.GetDictionariesAsync();
        return Ok(result);
    }
}
