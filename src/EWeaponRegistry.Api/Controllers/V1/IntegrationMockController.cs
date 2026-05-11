using EWeaponRegistry.Application.Interfaces.ExternalGateways;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EWeaponRegistry.Api.Controllers.V1;

/// <summary>
/// Mock integration endpoints for demonstration purposes.
/// These endpoints simulate external system integrations.
///
/// WARNING: These are MOCK implementations only.
/// Real integration with external systems would require:
/// - Proper authentication and authorization
/// - Secure communication channels
/// - Formal agreements and certifications
/// - Production-ready error handling
/// </summary>
[ApiController]
[Route("api/v1/integration/mock")]
[Authorize(Roles = "Admin")]
public class IntegrationMockController : ControllerBase
{
    private readonly INationalLoginGateway _nationalLoginGateway;
    private readonly IMObywatelGateway _mObywatelGateway;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IWpaExternalRegistryGateway _wpaExternalRegistryGateway;
    private readonly IPushNotificationGateway _pushNotificationGateway;

    public IntegrationMockController(
        INationalLoginGateway nationalLoginGateway,
        IMObywatelGateway mObywatelGateway,
        IPaymentGateway paymentGateway,
        IWpaExternalRegistryGateway wpaExternalRegistryGateway,
        IPushNotificationGateway pushNotificationGateway)
    {
        _nationalLoginGateway = nationalLoginGateway;
        _mObywatelGateway = mObywatelGateway;
        _paymentGateway = paymentGateway;
        _wpaExternalRegistryGateway = wpaExternalRegistryGateway;
        _pushNotificationGateway = pushNotificationGateway;
    }

    /// <summary>
    /// [MOCK] Simulate National Login (login.gov.pl) identity verification
    /// </summary>
    [HttpPost("national-login/verify")]
    [ProducesResponseType(typeof(NationalLoginVerifyResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<NationalLoginVerifyResult>> VerifyNationalLogin([FromBody] NationalLoginVerifyRequest request)
    {
        var result = await _nationalLoginGateway.VerifyIdentityAsync(request.Token);
        return Ok(result);
    }

    /// <summary>
    /// [MOCK] Simulate mObywatel QR code generation for promise
    /// </summary>
    [HttpPost("mobywatel/generate-qr")]
    [ProducesResponseType(typeof(MObywatelQrResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<MObywatelQrResult>> GenerateMObywatelQr([FromBody] GenerateQrRequest request)
    {
        var result = await _mObywatelGateway.GenerateQrTokenAsync(request.PromiseNumber, request.CitizenId);
        return Ok(result);
    }

    /// <summary>
    /// [MOCK] Simulate payment confirmation
    /// </summary>
    [HttpPost("payments/confirm")]
    [ProducesResponseType(typeof(PaymentConfirmResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentConfirmResult>> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
    {
        var result = await _paymentGateway.ConfirmPaymentAsync(request.PaymentId);
        return Ok(result);
    }

    /// <summary>
    /// [MOCK] Simulate WPA registry weapon book verification
    /// </summary>
    [HttpPost("wpa/verify-weapon-book")]
    [ProducesResponseType(typeof(WeaponBookVerifyResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<WeaponBookVerifyResult>> VerifyWeaponBook([FromBody] VerifyWeaponBookRequest request)
    {
        var result = await _wpaExternalRegistryGateway.VerifyWeaponBookNumberAsync(request.WeaponBookNumber, request.Pesel);
        return Ok(result);
    }

    /// <summary>
    /// [MOCK] Simulate push notification sending
    /// </summary>
    [HttpPost("push/send")]
    [ProducesResponseType(typeof(PushNotificationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<PushNotificationResult>> SendPushNotification([FromBody] SendPushRequest request)
    {
        var result = await _pushNotificationGateway.SendNotificationAsync(request.UserId, request.Title, request.Message);
        return Ok(result);
    }
}

// Request DTOs for mock endpoints
public class NationalLoginVerifyRequest
{
    public string Token { get; set; } = string.Empty;
}

public class GenerateQrRequest
{
    public string PromiseNumber { get; set; } = string.Empty;
    public Guid CitizenId { get; set; }
}

public class ConfirmPaymentRequest
{
    public string PaymentId { get; set; } = string.Empty;
}

public class VerifyWeaponBookRequest
{
    public string WeaponBookNumber { get; set; } = string.Empty;
    public string Pesel { get; set; } = string.Empty;
}

public class SendPushRequest
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
