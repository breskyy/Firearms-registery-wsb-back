using System.Security.Claims;
using EWeaponRegistry.Application.DTOs.Citizen;
using EWeaponRegistry.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWeaponRegistry.Api.Controllers.V1;

[ApiController]
[Route("api/v1/citizen")]
[Authorize(Roles = "Citizen")]
public class CitizenController : ControllerBase
{
    private readonly ICitizenService _citizenService;

    public CitizenController(ICitizenService citizenService)
    {
        _citizenService = citizenService;
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

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim!.Value);
    }
}
