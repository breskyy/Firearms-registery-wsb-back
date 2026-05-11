namespace EWeaponRegistry.Application.Interfaces.ExternalGateways;

/// <summary>
/// Gateway for push notification service integration.
/// NOTE: This is a MOCK interface. Real integration would require:
/// - Firebase Cloud Messaging, Apple Push Notifications, or similar service
/// - Device token management
/// - Proper error handling and retry logic
/// </summary>
public interface IPushNotificationGateway
{
    Task<PushNotificationResult> SendNotificationAsync(Guid userId, string title, string message);
    Task<PushNotificationResult> SendBulkNotificationAsync(IEnumerable<Guid> userIds, string title, string message);
}

public class PushNotificationResult
{
    public bool Success { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public string? ErrorMessage { get; set; }
}
