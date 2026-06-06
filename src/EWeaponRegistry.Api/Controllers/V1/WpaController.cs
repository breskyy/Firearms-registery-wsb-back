using System.Security.Claims;
using EWeaponRegistry.Application.DTOs.Common;
using EWeaponRegistry.Application.DTOs.Wpa;
using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Domain.Enums;
using EWeaponRegistry.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EWeaponRegistry.Api.Controllers.V1;

[ApiController]
[Route("api/v1/wpa")]
[Authorize(Roles = "WpaOfficer,Admin")]
public class WpaController : ControllerBase
{
    private readonly IWpaService _wpaService;
    private readonly IPermitMedicalExamRenewalService _renewalService;
    private readonly AppDbContext _context;

    public WpaController(
        IWpaService wpaService,
        IPermitMedicalExamRenewalService renewalService,
        AppDbContext context)
    {
        _wpaService = wpaService;
        _renewalService = renewalService;
        _context = context;
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
    /// Verify promise application payment after citizen submitted proof or mock payment
    /// </summary>
    [HttpPost("promise-applications/{id:guid}/verify-payment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> VerifyPromiseApplicationPayment(Guid id)
    {
        var officerId = GetUserId();
        await _wpaService.VerifyPromiseApplicationPaymentAsync(officerId, id);
        return NoContent();
    }

    /// <summary>
    /// Reject promise application payment proof and request resubmission
    /// </summary>
    [HttpPost("promise-applications/{id:guid}/reject-payment-proof")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RejectPromiseApplicationPaymentProof(
        Guid id,
        [FromBody] RejectPaymentProofRequest request)
    {
        var officerId = GetUserId();
        await _wpaService.RejectPromiseApplicationPaymentProofAsync(officerId, id, request.Comment);
        return NoContent();
    }

    /// <summary>
    /// Download a promise application attachment for officer verification
    /// </summary>
    [HttpGet("promise-applications/{applicationId:guid}/attachments/{attachmentId:guid}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadPromiseApplicationAttachment(Guid applicationId, Guid attachmentId)
    {
        var attachment = await _context.PromiseApplicationAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.PromiseApplicationId == applicationId);

        if (attachment == null)
            return NotFound();

        return File(attachment.Content, attachment.ContentType, attachment.FileName);
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

    /// <summary>
    /// Get permit applications (paginated, filterable by status)
    /// </summary>
    [HttpGet("permit-applications")]
    [ProducesResponseType(typeof(PaginatedResult<WpaPermitApplicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<WpaPermitApplicationDto>>> GetPermitApplications(
        [FromQuery] PermitApplicationStatus? status,
        [FromQuery] PaginationParams pagination)
    {
        var result = await _wpaService.GetPermitApplicationsAsync(status, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Get permit application details
    /// </summary>
    [HttpGet("permit-applications/{id:guid}")]
    [ProducesResponseType(typeof(WpaPermitApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WpaPermitApplicationDto>> GetPermitApplicationById(Guid id)
    {
        var result = await _wpaService.GetPermitApplicationByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Download a permit application attachment for officer verification
    /// </summary>
    [HttpGet("permit-applications/{applicationId:guid}/attachments/{attachmentId:guid}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadPermitApplicationAttachment(Guid applicationId, Guid attachmentId)
    {
        var attachment = await _context.PermitApplicationAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.PermitApplicationId == applicationId);

        if (attachment == null)
            return NotFound();

        return File(attachment.Content, attachment.ContentType, attachment.FileName);
    }

    /// <summary>
    /// Mark permit application as under review
    /// </summary>
    [HttpPost("permit-applications/{id:guid}/mark-under-review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarkPermitApplicationUnderReview(Guid id)
    {
        var officerId = GetUserId();
        await _wpaService.MarkPermitApplicationUnderReviewAsync(officerId, id);
        return NoContent();
    }

    /// <summary>
    /// Approve permit application (creates active permit)
    /// </summary>
    [HttpPost("permit-applications/{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ApprovePermitApplication(Guid id, [FromBody] ApprovePermitApplicationRequest request)
    {
        var officerId = GetUserId();
        await _wpaService.ApprovePermitApplicationAsync(officerId, id, request);
        return NoContent();
    }

    /// <summary>
    /// Reject permit application
    /// </summary>
    [HttpPost("permit-applications/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RejectPermitApplication(Guid id, [FromBody] ReviewPermitApplicationRequest? request)
    {
        var officerId = GetUserId();
        await _wpaService.RejectPermitApplicationAsync(officerId, id, request?.Reason);
        return NoContent();
    }

    /// <summary>
    /// Mark permit application as requiring correction
    /// </summary>
    [HttpPost("permit-applications/{id:guid}/require-correction")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RequirePermitApplicationCorrection(Guid id, [FromBody] ReviewPermitApplicationRequest? request)
    {
        var officerId = GetUserId();
        await _wpaService.RequirePermitApplicationCorrectionAsync(officerId, id, request?.Reason);
        return NoContent();
    }

    /// <summary>
    /// Verify permit application payment after citizen submitted proof or mock payment
    /// </summary>
    [HttpPost("permit-applications/{id:guid}/verify-payment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> VerifyPermitApplicationPayment(Guid id)
    {
        var officerId = GetUserId();
        await _wpaService.VerifyPermitApplicationPaymentAsync(officerId, id);
        return NoContent();
    }

    /// <summary>
    /// Reject permit application payment proof and request resubmission
    /// </summary>
    [HttpPost("permit-applications/{id:guid}/reject-payment-proof")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RejectPermitApplicationPaymentProof(
        Guid id,
        [FromBody] RejectPaymentProofRequest request)
    {
        var officerId = GetUserId();
        await _wpaService.RejectPermitApplicationPaymentProofAsync(officerId, id, request.Comment);
        return NoContent();
    }

    /// <summary>
    /// Suspend a permit
    /// </summary>
    [HttpPost("permits/{id:guid}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SuspendPermit(Guid id, [FromBody] ManagePermitRequest? request)
    {
        var officerId = GetUserId();
        await _wpaService.SuspendPermitAsync(officerId, id, request?.Reason);
        return NoContent();
    }

    /// <summary>
    /// Revoke a permit
    /// </summary>
    [HttpPost("permits/{id:guid}/revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RevokePermit(Guid id, [FromBody] ManagePermitRequest? request)
    {
        var officerId = GetUserId();
        await _wpaService.RevokePermitAsync(officerId, id, request?.Reason);
        return NoContent();
    }

    /// <summary>
    /// Restore a suspended permit
    /// </summary>
    [HttpPost("permits/{id:guid}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RestorePermit(Guid id, [FromBody] ManagePermitRequest? request)
    {
        var officerId = GetUserId();
        await _wpaService.RestorePermitAsync(officerId, id, request?.Reason);
        return NoContent();
    }

    /// <summary>
    /// List medical exam renewal submissions
    /// </summary>
    [HttpGet("medical-exam-renewals")]
    [ProducesResponseType(typeof(PaginatedResult<WpaPermitMedicalExamRenewalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<WpaPermitMedicalExamRenewalDto>>> GetMedicalExamRenewals(
        [FromQuery] PermitMedicalExamRenewalStatus? status,
        [FromQuery] PaginationParams pagination)
    {
        var result = await _renewalService.GetRenewalsForWpaAsync(status, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Get medical exam renewal details
    /// </summary>
    [HttpGet("medical-exam-renewals/{id:guid}")]
    [ProducesResponseType(typeof(WpaPermitMedicalExamRenewalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WpaPermitMedicalExamRenewalDto>> GetMedicalExamRenewal(Guid id)
    {
        var result = await _renewalService.GetRenewalByIdForWpaAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Download a renewal certificate attachment
    /// </summary>
    [HttpGet("medical-exam-renewals/{renewalId:guid}/attachments/{attachmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadMedicalExamRenewalAttachment(Guid renewalId, Guid attachmentId)
    {
        var file = await _renewalService.GetRenewalAttachmentAsync(renewalId, attachmentId, requireOfficer: true);
        if (file == null)
            return NotFound();

        return File(file.Value.Content, file.Value.ContentType, file.Value.FileName);
    }

    /// <summary>
    /// Mark renewal as under review
    /// </summary>
    [HttpPost("medical-exam-renewals/{id:guid}/mark-under-review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkMedicalExamRenewalUnderReview(Guid id)
    {
        await _renewalService.MarkRenewalUnderReviewAsync(GetUserId(), id);
        return NoContent();
    }

    /// <summary>
    /// Approve medical exam renewal
    /// </summary>
    [HttpPost("medical-exam-renewals/{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ApproveMedicalExamRenewal(Guid id, [FromBody] ApprovePermitMedicalExamRenewalRequest? request)
    {
        await _renewalService.ApproveRenewalAsync(GetUserId(), id, request ?? new ApprovePermitMedicalExamRenewalRequest());
        return NoContent();
    }

    /// <summary>
    /// Reject medical exam renewal
    /// </summary>
    [HttpPost("medical-exam-renewals/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RejectMedicalExamRenewal(Guid id, [FromBody] RejectPermitMedicalExamRenewalRequest request)
    {
        await _renewalService.RejectRenewalAsync(GetUserId(), id, request);
        return NoContent();
    }

    /// <summary>
    /// Update medical exam dates on a permit (registry override without citizen renewal)
    /// </summary>
    [HttpPatch("permits/{id:guid}/medical-exams")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePermitMedicalExams(Guid id, [FromBody] UpdatePermitMedicalExamsRequest request)
    {
        var officerId = GetUserId();
        await _wpaService.UpdatePermitMedicalExamsAsync(officerId, id, request);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim!.Value);
    }
}
