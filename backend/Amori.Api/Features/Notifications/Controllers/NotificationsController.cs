using Amori.Api.Common.Exceptions;
using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Amori.Api.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Features.Notifications.Controllers;

public sealed class RegisterDeviceRequest
{
    public string DeviceToken { get; set; } = string.Empty;
    public NotificationPlatform Platform { get; set; }
    public string? DeviceIdentifier { get; set; }
}

public sealed class DeviceRegistrationResponse
{
    public Guid Id { get; init; }
    public string DeviceToken { get; init; } = string.Empty;
    public NotificationPlatform Platform { get; init; }
    public string? DeviceIdentifier { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastSeenAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class NotificationResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? NotificationType { get; init; }
    public string? ReferenceId { get; init; }
    public bool IsRead { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>Device registration and in-app notification management.</summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(
    AmoriDbContext db,
    ICurrentUserService currentUser) : ControllerBase
{
    private Guid RequireUserId() =>
        currentUser.UserId ?? throw new UnauthorizedException();

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

    // ── Device Registration ───────────────────────────────────────────────────

    /// <summary>Register a device for push notifications.</summary>
    [HttpPost("devices")]
    [ProducesResponseType(typeof(DeviceRegistrationResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        if (string.IsNullOrWhiteSpace(req.DeviceToken)) throw new ValidationException("DeviceToken is required.");

        // Upsert — if same token already registered, update it
        var existing = await db.DeviceRegistrations
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceToken == req.DeviceToken, ct);

        if (existing != null)
        {
            existing.IsActive = true;
            existing.LastSeenAt = DateTime.UtcNow;
            existing.Platform = req.Platform;
            existing.DeviceIdentifier = req.DeviceIdentifier ?? existing.DeviceIdentifier;
            await db.SaveChangesAsync(ct);
            return Ok(MapDevice(existing));
        }

        var device = new DeviceRegistration
        {
            UserId = userId,
            DeviceToken = req.DeviceToken,
            Platform = req.Platform,
            DeviceIdentifier = req.DeviceIdentifier,
            LastSeenAt = DateTime.UtcNow
        };

        db.DeviceRegistrations.Add(device);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(RegisterDevice), new { }, MapDevice(device));
    }

    /// <summary>Deregister / remove a device.</summary>
    [HttpDelete("devices/{deviceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveDevice(Guid deviceId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var device = await db.DeviceRegistrations.FindAsync([deviceId], ct)
            ?? throw new NotFoundException("Device", deviceId);
        if (device.UserId != userId) throw new UnauthorizedException();
        db.DeviceRegistrations.Remove(device);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── In-App Notifications ──────────────────────────────────────────────────

    /// <summary>Get all notifications for the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(CancellationToken ct)
    {
        var userId = RequireUserId();
        var notifications = await db.AppNotifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
        return Ok(notifications.Select(MapNotification));
    }

    /// <summary>Mark a notification as read.</summary>
    [HttpPost("{notificationId:guid}/read")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkRead(Guid notificationId, CancellationToken ct)
    {
        var userId = RequireUserId();
        var n = await db.AppNotifications.FindAsync([notificationId], ct)
            ?? throw new NotFoundException("Notification", notificationId);
        if (n.UserId != userId) throw new UnauthorizedException();
        n.IsRead = true;
        n.ReadAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(MapNotification(n));
    }

    /// <summary>Mark all notifications as read.</summary>
    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var userId = RequireUserId();
        var unread = await db.AppNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);
        var now = DateTime.UtcNow;
        foreach (var n in unread) { n.IsRead = true; n.ReadAt = now; }
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
