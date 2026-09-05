using Amori.Api.Domain.Enums;

namespace Amori.Api.Domain.Entities;

public sealed class DeviceRegistration : BaseEntity
{
    public Guid UserId { get; set; }
    public string DeviceToken { get; set; } = string.Empty;
    public NotificationPlatform Platform { get; set; }
    public string? DeviceIdentifier { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastSeenAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
