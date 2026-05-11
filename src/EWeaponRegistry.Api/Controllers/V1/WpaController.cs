using System.Security.Claims;
using EWeaponRegistry.Application.DTOs.Common;
using EWeaponRegistry.Application.DTOs.Wpa;
using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWeaponRegistry.Api.Controllers.V1;

[ApiController]
[Route("api/v1/wpa")]
[Authorize(Roles = "WpaOfficer,Admin")]
public class WpaController : ControllerBase
{
    private readonly IWpaService _wpaService;

    public WpaController(IWpaService wpaService)
    {
        _wpaService = wpaService;
    }

    /// <summary>
    /// Get list of citizens (paginated)
    /// </summary>
    [HttpGet("citizens")]
    [ProducesResponseType(typeof(PaginatedResult<WpaCitizenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<WpaCitizenDto>>> GetCitizens([FromQuery] PaginationParams pagination)
    {
        var result = await _wpaService.GetCitizensAsync(pagination);
        return Ok(result);
    }

    /// <summary>
    /// Get citizen details by ID
    /// </summary>
    [HttpGet("citizens/{id:guid}")]
    [ProducesResponseType(typeof(WpaCitizenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WpaCitizenDto>> GetCitizenById(Guid id)
    {
        var result = await _wpaService.GetCitizenByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Search firearms by various criteria
    /// </summary>
    [HttpGet("firearms")]
    [ProducesResponseType(typeof(PaginatedResult<WpaFirearmSearchResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<WpaFirearmSearchResult>>> SearchFirearms(
        [FromQuery] string? serialNumber,
        [FromQuery] string? pesel,
        [FromQuery] string? permitNumber,
        [FromQuery] PermitType? permitType,
        [FromQuery] PaginationParams pagination)
    {
        var result = await _wpaService.SearchFirearmsAsync(serialNumber, pesel, permitNumber, permitType, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Get promise applications (paginated, filterable by status)
    /// </summary>
    [HttpGet("promise-applications")]
    [ProducesResponseType(typeof(PaginatedResult<WpaPromiseApplicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<WpaPromiseApplicationDto>>> GetPromiseApplications(
        [FromQuery] PromiseApplicationStatus? status,
        [FromQuery] PaginationParams pagination)
    {
        var result = await _wpaService.GetPromiseApplicationsAsync(status, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Get promise application details
    /// </summary>
    [HttpGet("promise-applications/{id:guid}")]
    [ProducesResponseType(typeof(WpaPromiseApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WpaPromiseApplicationDto>> GetPromiseApplicationById(Guid id)
    {
        var result = await _wpaService.GetPromiseApplicationByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Mark application as under review
    /// </summary>
    [HttpPost("promise-applications/{id:guid}/mark-under-review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarkApplicationUnderReview(Guid id)
    {
        var officerId = GetUserId();
        await _wpaService.MarkApplicationUnderReviewAsync(officerId, id);
        return NoContent();
    }

    /// <summary>
    /// Approve promise application (creates active promise)
    /// </summary>
    [HttpPost("promise-applications/{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ApproveApplication(Guid id)
    {
        var officerId = GetUserId();
        await _wpaService.ApproveApplicationAsync(officerId, id);
        return NoContent();
    }

    /// <summary>
    /// Reject promise application
    /// </summary>
    [HttpPost("promise-applications/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RejectApplication(Guid id, [FromBody] ReviewPromiseApplicationRequest? request)
    {
        var officerId = GetUserId();
        await _wpaService.RejectApplicationAsync(officerId, id, request?.Reason);
        return NoContent();
    }

    /// <summary>
    /// Mark application as requiring correction
    /// </summary>
    [HttpPost("promise-applications/{id:guid}/require-correction")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequireCorrection(Guid id, [FromBody] ReviewPromiseApplicationRequest? request)
    {
        var officerId = GetUserId();
        await _wpaService.RequireCorrectionAsync(officerId, id, request?.Reason);
        return NoContent();
    }

    /// <summary>
    /// Get medical alerts
    /// </summary>
    [HttpGet("medical-alerts")]
    [ProducesResponseType(typeof(PaginatedResult<MedicalAlertDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<MedicalAlertDto>>> GetMedicalAlerts(
        [FromQuery] bool? resolved,
        [FromQuery] PaginationParams pagination)
    {
        var result = await _wpaService.GetMedicalAlertsAsync(resolved, pagination);
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim!.Value);
    }
}
