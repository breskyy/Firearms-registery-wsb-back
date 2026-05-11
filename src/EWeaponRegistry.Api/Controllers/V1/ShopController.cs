using System.Security.Claims;
using EWeaponRegistry.Application.DTOs.Shop;
using EWeaponRegistry.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWeaponRegistry.Api.Controllers.V1;

[ApiController]
[Route("api/v1/shop")]
[Authorize(Roles = "Shop")]
public class ShopController : ControllerBase
{
    private readonly IShopService _shopService;

    public ShopController(IShopService shopService)
    {
        _shopService = shopService;
    }

    /// <summary>
    /// Verify buyer's permit/promise by QR token or promise number
    /// </summary>
    [HttpPost("verify-permit")]
    [ProducesResponseType(typeof(VerifyPermitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<VerifyPermitResponse>> VerifyPermit([FromBody] VerifyPermitRequest request)
    {
        var userId = GetUserId();
        var result = await _shopService.VerifyPermitAsync(userId, request);
        return Ok(result);
    }

    /// <summary>
    /// Register a firearm sale to a citizen
    /// </summary>
    /// <remarks>
    /// This operation is atomic and will:
    /// - Validate the promise is active and not expired
    /// - Validate the permit is active and has available slots
    /// - Validate medical/psychological exams are current
    /// - Verify the firearm category matches permit type
    /// - Check serial number uniqueness
    /// - Create the firearm record
    /// - Update promise usage
    /// - Update permit slots
    /// - Create ownership history
    /// - Log the audit trail
    /// </remarks>
    [HttpPost("firearms/register-sale")]
    [ProducesResponseType(typeof(RegisterSaleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterSaleResponse>> RegisterSale([FromBody] RegisterSaleRequest request)
    {
        var userId = GetUserId();
        var result = await _shopService.RegisterSaleAsync(userId, request);
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim!.Value);
    }
}
