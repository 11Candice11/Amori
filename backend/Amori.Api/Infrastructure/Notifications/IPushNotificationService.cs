namespace Amori.Api.Infrastructure.Notifications;

/// <summary>
/// Abstraction for push notifications (iOS/Android).
/// Implementation will use AWS SNS or a direct FCM/APNs integration.
/// </summary>
public interface IPushNotificationService
{
    Task SendAsync(string deviceToken, string title, string body, IDictionary<string, string>? data = null, CancellationToken cancellationToken = default);
    Task SendToUserAsync(Guid userId, string title, string body, IDictionary<string, string>? data = null, CancellationToken cancellationToken = default);
}
