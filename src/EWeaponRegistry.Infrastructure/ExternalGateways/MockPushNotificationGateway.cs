using EWeaponRegistry.Application.Interfaces.ExternalGateways;
using Microsoft.Extensions.Logging;

namespace EWeaponRegistry.Infrastructure.ExternalGateways;

/// <summary>
/// Mock implementation of Push Notification Gateway.
/// NOTE: This is a DEMONSTRATION ONLY.
/// Real integration would require:
/// - Firebase Cloud Messaging, Apple Push Notifications, or similar service
/// - Device token management
/// - Proper error handling and retry logic
/// </summary>
public class MockPushNotificationGateway : IPushNotificationGateway
{
    private readonly ILogger<MockPushNotificationGateway> _logger;

    public MockPushNotificationGateway(ILogger<MockPushNotificationGateway> logger)
    {
        _logger = logger;
    }

    public Task<PushNotificationResult> SendNotificationAsync(Guid userId, string title, string message)
    {
        _logger.LogInformation("[MOCK] Push notification sent to user {UserId}: {Title}", userId, title);

        return Task.FromResult(new PushNotificationResult
        {
            Success = true,
            SentCount = 1,
            FailedCount = 0
        });
    }

    public Task<PushNotificationResult> SendBulkNotificationAsync(IEnumerable<Guid> userIds, string title, string message)
    {
        var userIdList = userIds.ToList();
        _logger.LogInformation("[MOCK] Bulk push notification sent to {Count} users: {Title}", userIdList.Count, title);

        return Task.FromResult(new PushNotificationResult
        {
            Success = true,
            SentCount = userIdList.Count,
            FailedCount = 0
        });
    }
}
