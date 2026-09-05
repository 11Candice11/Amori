using Amori.Api.Features.Notifications.Controllers;

namespace Amori.Api.Features.Notifications.Services;

/// <summary>
/// Business logic for device registration and in-app notification management.
/// </summary>
public interface INotificationService
{
    Task<DeviceRegistrationResponse> RegisterDeviceAsync(Guid userId, RegisterDeviceRequest request, CancellationToken ct = default);
    Task RemoveDeviceAsync(Guid userId, Guid deviceId, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationResponse>> GetNotificationsAsync(Guid userId, CancellationToken ct = default);
    Task<NotificationResponse> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken ct = default);
    Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
}
