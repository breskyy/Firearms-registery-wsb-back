using System.Security.Claims;
using EWeaponRegistry.Application.DTOs.Citizen;
using EWeaponRegistry.Application.Interfaces;
using EWeaponRegistry.Domain.Entities;
using EWeaponRegistry.Domain.Enums;
using EWeaponRegistry.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EWeaponRegistry.Api.Controllers.V1;

[ApiController]
[Route("api/v1/citizen")]
[Authorize(Roles = "Citizen")]
public class CitizenController : ControllerBase
{
    private readonly ICitizenService _citizenService;
    private readonly AppDbContext _context;

    public CitizenController(ICitizenService citizenService, AppDbContext context)
    {
        _citizenService = citizenService;
        _context = context;
    }

    /// <summary>
    /// Get current citizen's profile
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CitizenProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CitizenProfileDto>> GetMyProfile()
    {
        var userId = GetUserId();
        var result = await _citizenService.GetMyProfileAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Get current citizen's permits
    /// </summary>
    [HttpGet("me/permits")]
    [ProducesResponseType(typeof(IList<PermitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<PermitDto>>> GetMyPermits()
    {
        var userId = GetUserId();
        var result = await _citizenService.GetMyPermitsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Get current citizen's registered firearms
    /// </summary>
    [HttpGet("me/firearms")]
    [ProducesResponseType(typeof(IList<FirearmDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<FirearmDto>>> GetMyFirearms()
    {
        var userId = GetUserId();
        var result = await _citizenService.GetMyFirearmsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Get specific firearm details with ownership history
    /// </summary>
    [HttpGet("me/firearms/{id:guid}")]
    [ProducesResponseType(typeof(FirearmDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FirearmDetailDto>> GetMyFirearmById(Guid id)
    {
        var userId = GetUserId();
        var result = await _citizenService.GetMyFirearmByIdAsync(userId, id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Get current citizen's promises (e-promesy)
    /// </summary>
    [HttpGet("me/promises")]
    [ProducesResponseType(typeof(IList<PromiseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<PromiseDto>>> GetMyPromises()
    {
        var userId = GetUserId();
        var result = await _citizenService.GetMyPromisesAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Get current citizen's promise applications
    /// </summary>
    [HttpGet("me/promise-applications")]
    [ProducesResponseType(typeof(IList<PromiseApplicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<PromiseApplicationDto>>> GetMyPromiseApplications()
    {
        var userId = GetUserId();
        var result = await _citizenService.GetMyPromiseApplicationsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Submit a new promise application
    /// </summary>
    [HttpPost("me/promise-applications")]
    [ProducesResponseType(typeof(PromiseApplicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PromiseApplicationDto>> CreatePromiseApplication([FromBody] CreatePromiseApplicationRequest request)
    {
        var userId = GetUserId();
        var result = await _citizenService.CreatePromiseApplicationAsync(userId, request);
        return CreatedAtAction(nameof(GetMyPromiseApplications), result);
    }

    /// <summary>
    /// Submit corrections for a promise application that requires correction
    /// </summary>
    [HttpPut("me/promise-applications/{id:guid}/correction")]
    [ProducesResponseType(typeof(PromiseApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PromiseApplicationDto>> UpdatePromiseApplicationCorrection(
        Guid id,
        [FromBody] UpdatePromiseApplicationCorrectionRequest request)
    {
        var userId = GetUserId();
        var result = await _citizenService.UpdatePromiseApplicationCorrectionAsync(userId, id, request);
        return Ok(result);
    }

    /// <summary>
    /// Get current citizen's transfer requests
    /// </summary>
    [HttpGet("me/transfer-requests")]
    [ProducesResponseType(typeof(IList<TransferRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<TransferRequestDto>>> GetMyTransferRequests()
    {
        var userId = GetUserId();
        var result = await _citizenService.GetMyTransferRequestsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Create a new transfer request (sell/donate firearm)
    /// </summary>
    [HttpPost("me/transfer-requests")]
    [ProducesResponseType(typeof(TransferRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransferRequestDto>> CreateTransferRequest([FromBody] CreateTransferRequest request)
    {
        var userId = GetUserId();
        var result = await _citizenService.CreateTransferRequestAsync(userId, request);
        return CreatedAtAction(nameof(GetMyTransferRequests), result);
    }

    /// <summary>
    /// Accept an incoming transfer request (as buyer)
    /// </summary>
    [HttpPost("me/transfer-requests/{id:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AcceptTransferRequest(Guid id)
    {
        var userId = GetUserId();
        await _citizenService.AcceptTransferRequestAsync(userId, id);
        return NoContent();
    }

    /// <summary>
    /// Reject an incoming transfer request (as buyer)
    /// </summary>
    [HttpPost("me/transfer-requests/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectTransferRequest(Guid id)
    {
        var userId = GetUserId();
        await _citizenService.RejectTransferRequestAsync(userId, id);
        return NoContent();
    }

    /// <summary>
    /// Cancel an outgoing transfer request (as seller)
    /// </summary>
    [HttpPost("me/transfer-requests/{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelTransferRequest(Guid id)
    {
        var userId = GetUserId();
        await _citizenService.CancelTransferRequestAsync(userId, id);
        return NoContent();
    }

    /// <summary>
    /// Get current citizen's permit applications
    /// </summary>
    [HttpGet("me/permit-applications")]
    [ProducesResponseType(typeof(IList<PermitApplicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<PermitApplicationDto>>> GetMyPermitApplications()
    {
        var userId = GetUserId();
        var result = await _citizenService.GetMyPermitApplicationsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Submit a new permit application
    /// </summary>
    [HttpPost("me/permit-applications")]
    [ProducesResponseType(typeof(PermitApplicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PermitApplicationDto>> CreatePermitApplication([FromBody] CreatePermitApplicationRequest request)
    {
        var userId = GetUserId();
        var result = await _citizenService.CreatePermitApplicationAsync(userId, request);
        return CreatedAtAction(nameof(GetMyPermitApplications), result);
    }

    /// <summary>
    /// Upload or replace medical certificate attachments for a permit application
    /// </summary>
    [HttpPost("me/permit-applications/{id:guid}/attachments")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(IList<PermitApplicationAttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IList<PermitApplicationAttachmentDto>>> UploadPermitApplicationAttachments(
        Guid id,
        [FromForm] IFormFile? medicalCertificate,
        [FromForm] IFormFile? psychologicalCertificate)
    {
        var userId = GetUserId();
        var citizen = await _context.CitizenProfiles.FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new EWeaponRegistry.Application.Exceptions.NotFoundException("Citizen profile not found");

        var application = await _context.PermitApplications
            .Include(pa => pa.Attachments)
            .FirstOrDefaultAsync(pa => pa.Id == id && pa.CitizenId == citizen.Id)
            ?? throw new EWeaponRegistry.Application.Exceptions.NotFoundException("Permit application", id);

        if (application.Status is PermitApplicationStatus.Approved or PermitApplicationStatus.Rejected)
            return Conflict(new { message = $"Cannot upload attachments for application in status {application.Status}" });

        if (medicalCertificate == null && psychologicalCertificate == null)
            return BadRequest(new { message = "At least one certificate file is required" });

        await ReplaceAttachmentAsync(application, medicalCertificate, PermitApplicationAttachmentType.MedicalCertificate);
        await ReplaceAttachmentAsync(application, psychologicalCertificate, PermitApplicationAttachmentType.PsychologicalCertificate);

        application.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var attachments = await _context.PermitApplicationAttachments
            .Where(a => a.PermitApplicationId == application.Id)
            .OrderBy(a => a.AttachmentType)
            .ToListAsync();

        return Ok(attachments.Select(a => new PermitApplicationAttachmentDto
        {
            Id = a.Id,
            AttachmentType = a.AttachmentType.ToString(),
            FileName = a.FileName,
            ContentType = a.ContentType,
            FileSize = a.FileSize,
            CreatedAt = a.CreatedAt
        }).ToList());
    }

    /// <summary>
    /// Submit corrections for a permit application that requires correction
    /// </summary>
    [HttpPut("me/permit-applications/{id:guid}/correction")]
    [ProducesResponseType(typeof(PermitApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PermitApplicationDto>> UpdatePermitApplicationCorrection(
        Guid id,
        [FromBody] UpdatePermitApplicationCorrectionRequest request)
    {
        var userId = GetUserId();
        var result = await _citizenService.UpdatePermitApplicationCorrectionAsync(userId, id, request);
        return Ok(result);
    }

    /// <summary>
    /// Get current citizen's medical alerts
    /// </summary>
    [HttpGet("me/medical-alerts")]
    [ProducesResponseType(typeof(IList<CitizenMedicalAlertDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<CitizenMedicalAlertDto>>> GetMyMedicalAlerts()
    {
        var userId = GetUserId();
        var result = await _citizenService.GetMyMedicalAlertsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Report a firearm as lost or stolen
    /// </summary>
    [HttpPost("me/firearms/{id:guid}/report-lost")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReportFirearmLost(Guid id, [FromBody] ReportFirearmLostRequest request)
    {
        var userId = GetUserId();
        await _citizenService.ReportFirearmLostAsync(userId, id, request);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim!.Value);
    }

    private async Task ReplaceAttachmentAsync(
        PermitApplication application,
        IFormFile? file,
        PermitApplicationAttachmentType type)
    {
        if (file == null || file.Length == 0)
            return;

        const long maxFileSize = 10 * 1024 * 1024;
        var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/png"
        };

        if (file.Length > maxFileSize)
            throw new EWeaponRegistry.Application.Exceptions.BusinessRuleViolationException("Attachment file cannot exceed 10 MB");

        if (!allowedContentTypes.Contains(file.ContentType))
            throw new EWeaponRegistry.Application.Exceptions.BusinessRuleViolationException("Only PDF, JPG and PNG files are allowed");

        var existing = application.Attachments.FirstOrDefault(a => a.AttachmentType == type);
        if (existing != null)
        {
            application.Attachments.Remove(existing);
            _context.PermitApplicationAttachments.Remove(existing);
        }

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);

        var attachment = new PermitApplicationAttachment
        {
            Id = Guid.NewGuid(),
            PermitApplicationId = application.Id,
            AttachmentType = type,
            FileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            FileSize = file.Length,
            Content = memory.ToArray(),
            CreatedAt = DateTime.UtcNow
        };

        await _context.PermitApplicationAttachments.AddAsync(attachment);
    }
}
