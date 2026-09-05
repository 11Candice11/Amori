using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Features.Notifications.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Notifications.Services;

public sealed class NotificationService(AmoriDbContext db) : INotificationService
{
    private static DeviceRegistrationResponse MapDevice(DeviceRegistration d) => new()
    {
        Id = d.Id, DeviceToken = d.DeviceToken, Platform = d.Platform,
        DeviceIdentifier = d.DeviceIdentifier, IsActive = d.IsActive,
        LastSeenAt = d.LastSeenAt, CreatedAt = d.CreatedAt
    };

    private static NotificationResponse MapNotification(AppNotification n) => new()
    {
        Id = n.Id, Title = n.Title, Body = n.Body,
        NotificationType = n.NotificationType, ReferenceId = n.ReferenceId,
        IsRead = n.IsRead, ReadAt = n.ReadAt, CreatedAt = n.CreatedAt
    };

    public async Task<DeviceRegistrationResponse> RegisterDeviceAsync(Guid userId, RegisterDeviceRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceToken)) throw new ValidationException("DeviceToken is required.");

        var existing = await db.DeviceRegistrations
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceToken == request.DeviceToken, ct);

        if (existing != null)
        {
            existing.IsActive = true;
            existing.LastSeenAt = DateTime.UtcNow;
            existing.Platform = request.Platform;
            existing.DeviceIdentifier = request.DeviceIdentifier ?? existing.DeviceIdentifier;
            await db.SaveChangesAsync(ct);
            return MapDevice(existing);
        }

        var device = new DeviceRegistration
        {
            UserId = userId, DeviceToken = request.DeviceToken,
            Platform = request.Platform, DeviceIdentifier = request.DeviceIdentifier,
            LastSeenAt = DateTime.UtcNow
        };
        db.DeviceRegistrations.Add(device);
        await db.SaveChangesAsync(ct);
        return MapDevice(device);
    }

    public async Task RemoveDeviceAsync(Guid userId, Guid deviceId, CancellationToken ct = default)
    {
        var device = await db.DeviceRegistrations.FindAsync([deviceId], ct)
            ?? throw new NotFoundException("Device", deviceId);
        if (device.UserId != userId) throw new UnauthorizedException();
        db.DeviceRegistrations.Remove(device);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<NotificationResponse>> GetNotificationsAsync(Guid userId, CancellationToken ct = default) =>
        await db.AppNotifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => MapNotification(n)).ToListAsync(ct);

    public async Task<NotificationResponse> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken ct = default)
    {
        var n = await db.AppNotifications.FindAsync([notificationId], ct)
            ?? throw new NotFoundException("Notification", notificationId);
        if (n.UserId != userId) throw new UnauthorizedException();
        n.IsRead = true;
        n.ReadAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return MapNotification(n);
    }

    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
    {
        var unread = await db.AppNotifications
            .Where(n => n.UserId == userId && !n.IsRead).ToListAsync(ct);
        var now = DateTime.UtcNow;
        foreach (var n in unread) { n.IsRead = true; n.ReadAt = now; }
        await db.SaveChangesAsync(ct);
    }
}
